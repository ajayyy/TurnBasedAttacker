using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour {

    /// <summary>
    /// Which player is this, used to know if it is this player's turn
    /// </summary>
    public int playerNum = 0;

    //true when selected by player, remains true when it is not this player's turn
    public bool selected = false; //public so it's default can be chosen in the editor

    //if stunned they cannot move for 3 turns
    public bool stunned = false;
    //turn the stun started on
    public int turnStunned = 0;
    //the gameobject that will hold the color placed over the player when stunned to be able to destroy it
    public GameObject stunnedColor;

    public Color highlightColor = new Color(1, 0, 0);
    public Color shootColor = new Color(0, 0, 1);
    public Color idleColor = new Color(0, 0, 0);

    public SpriteRenderer spriteRenderer;

    AnimationScript playerAnimation;
    Animator animator;

    //these are set from gamecontroller varivables, they are set in Start()
    GameObject projectile;
    // prefab
    GameObject slowProjectile;

    bool doneTurn = false; //if true, and the animation state is idle, then go to next turn

    //info on what the player is holding
    public int pickup = 0;
    public bool holding = false; //true when holding

    //if true, waiting for input for the direction
    bool shootMode = false;

    //if true, wait for input for the direction
    bool blockMode = false;

    //if true, wait for input for the direction of the spawn for the new player
    bool spawnMode = false;

    //if true, wait for input for the direction
    bool slowShootMode = false;

    //if true, waiting for input for the direction
    bool blockShootMode = false;

    //if true, waiting for input for the direction
    bool stunShootMode = false;

    void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = idleColor;

        playerAnimation = GetComponent<AnimationScript>();
        animator = GetComponent<Animator>();

        projectile = GameController.instance.projectile;
        slowProjectile = GameController.instance.slowProjectile;
    }

    void Update () {
		//if (playerNum != 15)
		//	GetComponent<AnimationScript> ().OnDeadAnimationEnded();
		
        //shothand for GameController.instance
        GameController gameController = GameController.instance;

		if (gameController.gameOver) {
			//the game is done, the player should not be able to move
			return;
		}

        if(stunned && gameController.turnNum - 5 >= turnStunned) {
            stunned = false;
            //Destroy(stunnedColor);
            gameController.usablePlayers[playerNum] = true;

            //setup fade animation
            Color stunColor = stunnedColor.GetComponent<SpriteRenderer>().color;
            stunnedColor.GetComponent<AnimationScript>().type = 5;
            stunnedColor.GetComponent<AnimationScript>().targetColor = new Color(stunColor.r, stunColor.g, stunColor.b, 0);
            stunnedColor.GetComponent<AnimationScript>().kill = true;

            stunnedColor.GetComponent<Animator>().SetTrigger("move");
        }

        //if it's this player's turn but it is animating
        if (gameController.turnPlayerNum == playerNum && Time.time - gameController.lastMove >= 0.01f && selected && !EverythingIdle() && !stunned) {
            //make the button disabled
            gameController.saveGameUsable = false;
        }

        //set this player as selected if it was clicked by the controlling player (LAN game)
        if (GameSettings.serverSocket != null && playerNum != 0 && !selected) {
            string message = GameSettings.connectedPlayers[playerNum - 1].GetMessage();
            bool correctMessage = message != null && message.Contains("selected: ");

            if (correctMessage) {
                int selectedPlayer = int.Parse(message.Split('{')[1].Split('}')[0]);

                if(selectedPlayer == gameController.players.IndexOf(gameObject)) {
                    GameSettings.connectedPlayers[playerNum - 1].RemoveMessage();

                    SelectPlayer();

                    GameSettings.SendToAllExcept(message, GameSettings.connectedPlayers[playerNum - 1]);
                }
            }
        }
        if (GameSettings.connectedServer != null && playerNum != GameSettings.currentPlayerNum && !selected) {
            string message = GameSettings.connectedServer.GetMessage();
            bool correctMessage = message != null && message.Contains("selected: ");

            if (correctMessage) {
                int selectedPlayer = int.Parse(message.Split('{')[1].Split('}')[0]);

                if (selectedPlayer == gameController.players.IndexOf(gameObject)) {
                    GameSettings.connectedServer.RemoveMessage();

                    SelectPlayer();
                }
            }
        }

        //if it is this player's turn
        if (gameController.turnPlayerNum == playerNum && Time.time - gameController.lastMove >= 0.01f && selected && EverythingIdle() && !stunned) {

            //make button enabled if we can play
            if(Time.time - gameController.lastMove <= 0.5f && !doneTurn) {
                gameController.saveGameUsable = true;
            }

            if (doneTurn) {
                gameController.NextTurn();

                ChangeColor(idleColor);

                doneTurn = false;
            } else if (shootMode) {
                ChangeColor(shootColor);

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Right()) {
                    chosen = true;
                    direction = 0;
                } else if (Left()) {
                    chosen = true;
                    direction = 180;
                } else if (Up()) {
                    chosen = true;
                    direction = 90;
                } else if (Down()) {
                    chosen = true;
                    direction = 270;
                }

                if (Action()) {
                    //disable it, they activated it by mistake
                    shootMode = false;
                }

                if (chosen) {
                    //find other player
                    RaycastHit2D otherPlayer = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(direction), MathHelper.DegreeToVector2(direction));

                    if (otherPlayer.collider != null && (otherPlayer.collider.tag == "Player" || otherPlayer.collider.tag == "Block")) {
                        projectile.GetComponent<AnimationScript>().direction = direction;
                        projectile.GetComponent<AnimationScript>().target = otherPlayer.collider.transform.position;
                        projectile.GetComponent<AnimationScript>().targetObject = otherPlayer.collider.gameObject;
                        projectile.transform.position = transform.position;
                        projectile.SetActive(true);

                        projectile.GetComponent<Animator>().SetTrigger("move");
                        doneTurn = true;
                        shootMode = false;
                        holding = false;
                    }

                }

            } else if (blockMode) {
                ChangeColor(shootColor);

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Right()) {
                    chosen = true;
                    direction = 0;
                } else if (Left()) {
                    chosen = true;
                    direction = 180;
                } else if (Up()) {
                    chosen = true;
                    direction = 90;
                } else if (Down()) {
                    chosen = true;
                    direction = 270;
                }

                if (Action()) {
                    //disable it, they activated it by mistake
                    blockMode = false;
                }

                if (chosen) {
                    //find other player
                    RaycastHit2D otherPlayer = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(direction), MathHelper.DegreeToVector2(direction));

                    GameObject newBlock = Instantiate(gameController.block);
                    newBlock.GetComponent<AnimationScript>().direction = direction;
                    newBlock.transform.position = transform.position;

                    gameController.blocks.Add(newBlock);

                    doneTurn = true;
                    blockMode = false;
                    holding = false;
                }

            } else if (spawnMode) {
                ChangeColor(shootColor);

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Right()) {
                    chosen = true;
                    direction = 0;
                } else if (Left()) {
                    chosen = true;
                    direction = 180;
                } else if (Up()) {
                    chosen = true;
                    direction = 90;
                } else if (Down()) {
                    chosen = true;
                    direction = 270;
                }

                if (Action()) {
                    //disable it, they activated it by mistake
                    spawnMode = false;
                }

                if (chosen) {
                    //find other player
                    RaycastHit2D otherPlayer = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(direction), MathHelper.DegreeToVector2(direction));
                    
                    GameObject newPlayer = Instantiate(gameController.player);
                    newPlayer.GetComponent<AnimationScript>().direction = direction;
                    newPlayer.transform.position = transform.position;

                    Player playerScript = newPlayer.GetComponent<Player>();
                    playerScript.playerNum = playerNum;
                    playerScript.idleColor = idleColor;
                    playerScript.highlightColor = highlightColor;
                    playerScript.shootColor = shootColor;
                    playerScript.selected = false;

                    playerScript.GetComponent<Animator>().SetTrigger("move");

                    gameController.players.Add(newPlayer);
                    GameController.instance.playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text = int.Parse(GameController.instance.playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text) + 1 + "";

                    doneTurn = true;
                    spawnMode = false;
                    holding = false;
                }

            } else if (slowShootMode) {
                ChangeColor(shootColor);

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Right()) {
                    chosen = true;
                    direction = 0;
                } else if (Left()) {
                    chosen = true;
                    direction = 180;
                } else if (Up()) {
                    chosen = true;
                    direction = 90;
                } else if (Down()) {
                    chosen = true;
                    direction = 270;
                }

                if (Action()) {
                    //disable it, they activated it by mistake
                    slowShootMode = false;
                }

                if (chosen) {
                    //find other player

                    GameObject newProjectile = Instantiate(slowProjectile);

                    SlowProjectile slowProjectileScript = newProjectile.GetComponent<SlowProjectile>();

                    slowProjectileScript.direction = direction;
                    slowProjectileScript.lastTurnMoved = playerNum;
                    newProjectile.transform.position = transform.position;

                    gameController.slowProjectiles.Add(newProjectile);


                    doneTurn = true;
                    slowShootMode = false;
                    holding = false;

                }

            } else if (blockShootMode) {
                ChangeColor(shootColor);

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Right()) {
                    chosen = true;
                    direction = 0;
                } else if (Left()) {
                    chosen = true;
                    direction = 180;
                } else if (Up()) {
                    chosen = true;
                    direction = 90;
                } else if (Down()) {
                    chosen = true;
                    direction = 270;
                }

                if (Action()) {
                    //disable it, they activated it by mistake
                    blockShootMode = false;
                }

                if (chosen) {
                    //find other player
                    RaycastHit2D otherPlayer = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(direction), MathHelper.DegreeToVector2(direction));

                    if (otherPlayer.collider != null) {
                        gameController.blockProjectile.GetComponent<AnimationScript>().direction = direction;
                        gameController.blockProjectile.GetComponent<AnimationScript>().target = otherPlayer.collider.transform.position;
                        if(otherPlayer.collider.gameObject.tag == "Border") {
                            gameController.blockProjectile.GetComponent<AnimationScript>().target = otherPlayer.point;
                        }
                        gameController.blockProjectile.GetComponent<AnimationScript>().targetObject = otherPlayer.collider.gameObject;
                        gameController.blockProjectile.transform.position = transform.position;
                        gameController.blockProjectile.SetActive(true);

                        gameController.blockProjectile.GetComponent<Animator>().SetTrigger("move");
                        doneTurn = true;
                        blockShootMode = false;
                        holding = false;
                    }

                }

            } else if (stunShootMode) {
                ChangeColor(shootColor);

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Right()) {
                    chosen = true;
                    direction = 0;
                } else if (Left()) {
                    chosen = true;
                    direction = 180;
                } else if (Up()) {
                    chosen = true;
                    direction = 90;
                } else if (Down()) {
                    chosen = true;
                    direction = 270;
                }

                if (Action()) {
                    //disable it, they activated it by mistake
                    stunShootMode = false;
                }

                if (chosen) {
                    //find other player
                    RaycastHit2D otherPlayer = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(direction), MathHelper.DegreeToVector2(direction));

                    if (otherPlayer.collider != null && otherPlayer.collider.gameObject.tag == "Player") {
                        gameController.stunProjectile.GetComponent<AnimationScript>().direction = direction;
                        gameController.stunProjectile.GetComponent<AnimationScript>().target = otherPlayer.collider.transform.position;
                        if (otherPlayer.collider.gameObject.tag == "Border") {
                            gameController.stunProjectile.GetComponent<AnimationScript>().target = otherPlayer.point;
                        }
                        gameController.stunProjectile.GetComponent<AnimationScript>().targetObject = otherPlayer.collider.gameObject;
                        gameController.stunProjectile.transform.position = transform.position;
                        gameController.stunProjectile.SetActive(true);

                        Player playerScript = otherPlayer.collider.gameObject.GetComponent<Player>();

                        if (int.Parse(GameController.instance.playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text) == 1) {
                            GameController.instance.usablePlayers[playerScript.playerNum] = false;
                        }

                        gameController.stunProjectile.GetComponent<Animator>().SetTrigger("move");
                        doneTurn = true;
                        stunShootMode = false;
                        holding = false;
                    }

                }

            } else {

                ChangeColor(highlightColor);

                bool moved = false;

                //movement
                if (Right()) {
                    playerAnimation.direction = 0;
                    playerAnimation.type = 0;
                    moved = true;
                } else if (Left()) {
                    playerAnimation.direction = 180;
                    playerAnimation.type = 0;
                    moved = true;
                } else if (Up()) {
                    playerAnimation.direction = 90;
                    playerAnimation.type = 0;
                    moved = true;
                } else if (Down()) {
                    playerAnimation.direction = 270;
                    playerAnimation.type = 0;
                    moved = true;
                }

                //projectiles
                if (Action() && holding && pickup == 0) {
                    shootMode = true;
                }

                //place block
                if (Action() && holding && pickup == 1) {
                    blockMode = true;
                }

                //spawn player
                if (Action() && holding && pickup == 2) {
                    spawnMode = true;
                }

                //slow projectile
                if (Action() && holding && pickup == 3) {
                    slowShootMode = true;
                }

                //block projectile
                if (Action() && holding && pickup == 4) {
                    blockShootMode = true;
                }

                //stun projectile
                if (Action() && holding && pickup == 5) {
                    stunShootMode = true;
                }

                if (moved) {
                    //check if there is something in the way
                    RaycastHit2D otherObject = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(playerAnimation.direction), MathHelper.DegreeToVector2(playerAnimation.direction));

                    if(otherObject.collider == null || (Vector3.Distance(otherObject.point, transform.position + MathHelper.DegreeToVector3(playerAnimation.direction)) > 0.6f || (otherObject.collider.tag != "Border" && Vector3.Distance(otherObject.point, transform.position + MathHelper.DegreeToVector3(playerAnimation.direction)) >= 0.45f)) || otherObject.collider.tag == "Pickup") {
                        animator.SetTrigger("move");
                        doneTurn = true; //once the animation becomes idle again, the doneTurn if statement will be triggered, and the next turn will start
                    }

                }
            }

        }

    }

    void OnMouseDown() {
        //check if this is connected or hosting a LAN game
        if(GameSettings.serverSocket != null && playerNum != 0) {
            return;
        }
        if(GameSettings.serverSocket != null && playerNum == 0) {
            GameSettings.SendToAll("selected: {" + GameController.instance.players.IndexOf(gameObject) + "}");
        }

        if (GameSettings.connectedServer != null && playerNum != GameSettings.currentPlayerNum) {
            return;
        }
        if (GameSettings.connectedServer != null && playerNum == GameSettings.currentPlayerNum) {
            GameSettings.connectedServer.SendMessage("selected: {" + GameController.instance.players.IndexOf(gameObject) + "}");
        }

        SelectPlayer();
    }
    
    //select this player and deselect all others
    public void SelectPlayer() {
        selected = true;

        foreach (GameObject playerObject in GameController.instance.players) {
            Player player = playerObject.GetComponent<Player>();
            if (player.playerNum == playerNum && player != this) {
                //this player is part of this person's characters, they are not selected because this is now selected
                player.selected = false;

                player.ChangeColor(player.idleColor);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag.Equals("Pickup")) {
            switch (collider.GetComponent<Pickup>().type) {
                case 0:
                    //TODO display some marker on the player that it can now shoot a projectile
                    break;
            }

            pickup = collider.GetComponent<Pickup>().type;
            holding = true;

            GameController.instance.pickups.Remove(collider.gameObject);
            Destroy(collider.gameObject);
        }
    }

    void ChangeColor(Color newColor) {
        if (spriteRenderer.color != newColor) {
            //fade color to highlight color

            GetComponent<AnimationScript>().type = 5;
            GetComponent<AnimationScript>().targetColor = newColor;

            GetComponent<Animator>().SetTrigger("move");
        }
    }

	bool EverythingIdle() {

		GameController gameController = GameController.instance;

		return animator.GetCurrentAnimatorStateInfo (0).IsName("idle") && (!projectile.activeSelf || projectile.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("idle")) && (!gameController.blockProjectile.activeSelf || gameController.blockProjectile.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("idle")) && (!gameController.stunProjectile.activeSelf || gameController.stunProjectile.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("idle"));
	}

    bool Up() {
        if(playerNum > 0 && GameSettings.serverSocket != null) {
            string message = GameSettings.connectedPlayers[playerNum - 1].GetMessage();
            bool correctMessage = message != null && message.Contains("m:w");
            if (correctMessage) {
                GameSettings.connectedPlayers[playerNum - 1].RemoveMessage();

                GameSettings.SendToAllExcept(message, GameSettings.connectedPlayers[playerNum - 1]);
            }
            return correctMessage;
        }
        if (playerNum == 0 && GameSettings.serverSocket != null && Input.GetKeyDown(KeyCode.W)) {
            foreach (ConnectedSocket connectedPlayer in GameSettings.connectedPlayers) {
                connectedPlayer.SendMessage("m:w;");
            }
        }


        if (playerNum != GameSettings.currentPlayerNum && GameSettings.connectedServer != null) {
            string message = GameSettings.connectedServer.GetMessage();
            bool correctMessage = message != null && message.Contains("m:w");
            if (correctMessage) {
                GameSettings.connectedServer.RemoveMessage();
            }
            return correctMessage;
        }
        if (playerNum == GameSettings.currentPlayerNum && GameSettings.connectedServer != null && Input.GetKeyDown(KeyCode.W)) {
            GameSettings.connectedServer.SendMessage("m:w;");
        }

        return Input.GetKeyDown(KeyCode.W);
    }

    bool Down() {
        if (playerNum > 0 && GameSettings.serverSocket != null) {
            string message = GameSettings.connectedPlayers[playerNum - 1].GetMessage();
            bool correctMessage = message != null && message.Contains("m:s");
            if (correctMessage) {
                GameSettings.connectedPlayers[playerNum - 1].RemoveMessage();

                GameSettings.SendToAllExcept(message, GameSettings.connectedPlayers[playerNum - 1]);
            }
            return correctMessage;
        }
        if (playerNum == 0 && GameSettings.serverSocket != null && Input.GetKeyDown(KeyCode.S)) {
            foreach (ConnectedSocket connectedPlayer in GameSettings.connectedPlayers) {
                connectedPlayer.SendMessage("m:s;");
            }
        }

        if (playerNum != GameSettings.currentPlayerNum && GameSettings.connectedServer != null) {
            string message = GameSettings.connectedServer.GetMessage();
            bool correctMessage = message != null && message.Contains("m:s");
            if (correctMessage) {
                GameSettings.connectedServer.RemoveMessage();
            }
            return correctMessage;
        }
        if (playerNum == GameSettings.currentPlayerNum && GameSettings.connectedServer != null && Input.GetKeyDown(KeyCode.S)) {
            GameSettings.connectedServer.SendMessage("m:s;");
        }

        return Input.GetKeyDown(KeyCode.S);
    }

    bool Right() {
        if (playerNum > 0 && GameSettings.serverSocket != null) {
            string message = GameSettings.connectedPlayers[playerNum - 1].GetMessage();
            bool correctMessage = message != null && message.Contains("m:d");
            if (correctMessage) {
                GameSettings.connectedPlayers[playerNum - 1].RemoveMessage();

                GameSettings.SendToAllExcept(message, GameSettings.connectedPlayers[playerNum - 1]);
            }
            return correctMessage;
        }
        if (playerNum == 0 && GameSettings.serverSocket != null && Input.GetKeyDown(KeyCode.D)) {
            foreach (ConnectedSocket connectedPlayer in GameSettings.connectedPlayers) {
                connectedPlayer.SendMessage("m:d;");
            }
        }

        if (playerNum != GameSettings.currentPlayerNum && GameSettings.connectedServer != null) {
            string message = GameSettings.connectedServer.GetMessage();
            bool correctMessage = message != null && message.Contains("m:d");
            if (correctMessage) {
                GameSettings.connectedServer.RemoveMessage();
            }
            return correctMessage;
        }
        if (playerNum == GameSettings.currentPlayerNum && GameSettings.connectedServer != null && Input.GetKeyDown(KeyCode.D)) {
            GameSettings.connectedServer.SendMessage("m:d;");
        }

        return Input.GetKeyDown(KeyCode.D);
    }

    bool Left() {
        if (playerNum > 0 && GameSettings.serverSocket != null) {
            string message = GameSettings.connectedPlayers[playerNum - 1].GetMessage();
            bool correctMessage = message != null && message.Contains("m:a");
            if (correctMessage) {
                GameSettings.connectedPlayers[playerNum - 1].RemoveMessage();

                GameSettings.SendToAllExcept(message, GameSettings.connectedPlayers[playerNum - 1]);
            }
            return correctMessage;
        }
        if (playerNum == 0 && GameSettings.serverSocket != null && Input.GetKeyDown(KeyCode.A)) {
            foreach (ConnectedSocket connectedPlayer in GameSettings.connectedPlayers) {
                connectedPlayer.SendMessage("m:a;");
            }
        }

        if (playerNum != GameSettings.currentPlayerNum && GameSettings.connectedServer != null) {
            string message = GameSettings.connectedServer.GetMessage();
            bool correctMessage = message != null && message.Contains("m:a");
            if (correctMessage) {
                GameSettings.connectedServer.RemoveMessage();
            }
            return correctMessage;
        }
        if (playerNum == GameSettings.currentPlayerNum && GameSettings.connectedServer != null && Input.GetKeyDown(KeyCode.A)) {
            GameSettings.connectedServer.SendMessage("m:a;");
        }

        return Input.GetKeyDown(KeyCode.A);
    }

    bool Action() {
        if (playerNum > 0 && GameSettings.serverSocket != null) {
            string message = GameSettings.connectedPlayers[playerNum - 1].GetMessage();
            bool correctMessage = message != null && message.Contains("m:e");
            if (correctMessage) {
                GameSettings.connectedPlayers[playerNum - 1].RemoveMessage();

                GameSettings.SendToAllExcept(message, GameSettings.connectedPlayers[playerNum - 1]);
            }
            return correctMessage;
        }
        if (playerNum == 0 && GameSettings.serverSocket != null && Input.GetKeyDown(KeyCode.E)) {
            foreach (ConnectedSocket connectedPlayer in GameSettings.connectedPlayers) {
                connectedPlayer.SendMessage("m:e;");
            }
        }

        if (playerNum != GameSettings.currentPlayerNum && GameSettings.connectedServer != null) {
            string message = GameSettings.connectedServer.GetMessage();
            bool correctMessage = message != null && message.Contains("m:e");
            if (correctMessage) {
                GameSettings.connectedServer.RemoveMessage();
            }
            return correctMessage;
        }
        if (playerNum == GameSettings.currentPlayerNum && GameSettings.connectedServer != null && Input.GetKeyDown(KeyCode.E)) {
            GameSettings.connectedServer.SendMessage("m:e;");
        }

        return Input.GetKeyDown(KeyCode.E);
    }

}
