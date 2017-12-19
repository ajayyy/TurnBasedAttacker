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

    public Color highlightColor = new Color(100, 0, 0);
    Color shootColor = new Color(0, 0, 100);
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
    int pickup = 0;
    bool holding = false; //true when holding

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
        //shothand for GameController.instance
        GameController gameController = GameController.instance;

        if(stunned && gameController.turnNum - 3 >= turnStunned) {
            stunned = false;
            Destroy(stunnedColor);
        }

        //if it is this player's turn
        if(gameController.turnPlayerNum == playerNum && Time.time - gameController.lastMove >= 0.01f && selected && animator.GetCurrentAnimatorStateInfo(0).IsName("idle") && !stunned) {
            if (doneTurn) {
                gameController.NextTurn();
                spriteRenderer.color = idleColor;
                doneTurn = false;
            } else if (shootMode) {
                spriteRenderer.color = shootColor;
                
                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Input.GetKeyDown(KeyCode.D)) {
                    chosen = true;
                    direction = 0;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    chosen = true;
                    direction = 180;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    chosen = true;
                    direction = 90;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    chosen = true;
                    direction = 270;
                }

                if (Input.GetKeyDown(KeyCode.E)) {
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
                spriteRenderer.color = shootColor;

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Input.GetKeyDown(KeyCode.D)) {
                    chosen = true;
                    direction = 0;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    chosen = true;
                    direction = 180;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    chosen = true;
                    direction = 90;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    chosen = true;
                    direction = 270;
                }

                if (Input.GetKeyDown(KeyCode.E)) {
                    //disable it, they activated it by mistake
                    blockMode = false;
                }

                if (chosen) {
                    //find other player
                    RaycastHit2D otherPlayer = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(direction), MathHelper.DegreeToVector2(direction));

                    GameObject newBlock = Instantiate(gameController.block);
                    newBlock.GetComponent<AnimationScript>().direction = direction;
                    newBlock.transform.position = transform.position;

                    doneTurn = true;
                    blockMode = false;
                    holding = false;
                }

            } else if (spawnMode) {
                spriteRenderer.color = shootColor;

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Input.GetKeyDown(KeyCode.D)) {
                    chosen = true;
                    direction = 0;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    chosen = true;
                    direction = 180;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    chosen = true;
                    direction = 90;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    chosen = true;
                    direction = 270;
                }

                if (Input.GetKeyDown(KeyCode.E)) {
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
                spriteRenderer.color = shootColor;

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Input.GetKeyDown(KeyCode.D)) {
                    chosen = true;
                    direction = 0;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    chosen = true;
                    direction = 180;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    chosen = true;
                    direction = 90;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    chosen = true;
                    direction = 270;
                }

                if (Input.GetKeyDown(KeyCode.E)) {
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


                    doneTurn = true;
                    slowShootMode = false;
                    holding = false;

                }

            } else if (blockShootMode) {
                spriteRenderer.color = shootColor;

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Input.GetKeyDown(KeyCode.D)) {
                    chosen = true;
                    direction = 0;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    chosen = true;
                    direction = 180;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    chosen = true;
                    direction = 90;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    chosen = true;
                    direction = 270;
                }

                if (Input.GetKeyDown(KeyCode.E)) {
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
                spriteRenderer.color = shootColor;

                bool chosen = false; //was a direction chosen
                float direction = 0; //the direction chosen in angles

                if (Input.GetKeyDown(KeyCode.D)) {
                    chosen = true;
                    direction = 0;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    chosen = true;
                    direction = 180;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    chosen = true;
                    direction = 90;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    chosen = true;
                    direction = 270;
                }

                if (Input.GetKeyDown(KeyCode.E)) {
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

                        gameController.stunProjectile.GetComponent<Animator>().SetTrigger("move");
                        doneTurn = true;
                        stunShootMode = false;
                        holding = false;
                    }

                }

            } else {
                spriteRenderer.color = highlightColor;

                bool moved = false;

                //movement
                if (Input.GetKeyDown(KeyCode.D)) {
                    playerAnimation.direction = 0;
                    playerAnimation.type = 0;
                    moved = true;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    playerAnimation.direction = 180;
                    playerAnimation.type = 0;
                    moved = true;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    playerAnimation.direction = 90;
                    playerAnimation.type = 0;
                    moved = true;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    playerAnimation.direction = 270;
                    playerAnimation.type = 0;
                    moved = true;
                }

                //projectiles
                if (Input.GetKeyDown(KeyCode.E) && holding && pickup == 0) {
                    shootMode = true;
                }

                //place block
                if (Input.GetKeyDown(KeyCode.E) && holding && pickup == 1) {
                    blockMode = true;
                }

                //spawn player
                if (Input.GetKeyDown(KeyCode.E) && holding && pickup == 2) {
                    spawnMode = true;
                }

                //slow projectile
                if (Input.GetKeyDown(KeyCode.E) && holding && pickup == 3) {
                    slowShootMode = true;
                }

                //block projectile
                if (Input.GetKeyDown(KeyCode.E) && holding && pickup == 4) {
                    blockShootMode = true;
                }

                //stun projectile
                if (Input.GetKeyDown(KeyCode.E) && holding && pickup == 5) {
                    stunShootMode = true;
                }

                if (moved) {
                    //check if there is something in the way
                    RaycastHit2D otherObject = Physics2D.Raycast(transform.position + MathHelper.DegreeToVector3(playerAnimation.direction), MathHelper.DegreeToVector2(playerAnimation.direction));

                    if(otherObject.collider == null || Vector3.Distance(otherObject.point, transform.position + MathHelper.DegreeToVector3(playerAnimation.direction)) > 0.6f || otherObject.collider.tag.Equals("Pickup")) {
                        animator.SetTrigger("move");
                        doneTurn = true; //once the animation becomes idle again, the doneTurn if statement will be triggered, and the next turn will start
                    }

                }
            }

        }

    }

    void OnMouseDown() {
        selected = true;
        
        foreach(GameObject playerObject in GameController.instance.players) {
            Player player = playerObject.GetComponent<Player>();
            if(player.playerNum == playerNum && player != this) {
                //this player is part of this person's characters, they are not selected because this is now selected
                player.selected = false;
                player.spriteRenderer.color = player.idleColor;
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

            Destroy(collider.gameObject);
        }
    }

}
