using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController instance = null;

    //set in awake
    public string gameId;

    //contains all players, including if a player has multiple characters on screen
    public List<GameObject> players = new List<GameObject>();

    //true if not stunned
    public bool[] usablePlayers;

	// Amount of actual people playing/turns to go through. Because the players array now includes more than one player per person
    public int personAmount = 0;

	//Amount of players with no units left
	public int playersDead = 0;

	//stores all the player nums in the order they died (the last player class that was left
	public List<Player> playersDeadList = new List<Player>();

	//if the game is over and it is showing the scoreboard
	public bool gameOver = false;

    //Actual turn number
    public int turnNum = 0;

    //The text gameobject displaying the turn number
    public Text turnNumText;

    ///<summary>
    /// What player's turn is it
    ///</summary>
    public int turnPlayerNum = 0;

    //a list of all player idle colors selected in the inspector
    public Color[] playerColors;

    /// <summary>
    /// Time of last movment (to make a delay of when you can move again
    /// </summary>
    public float lastMove;

    //the projectile in this scene
    public GameObject projectile;

    //the slow projectile prefab
    public GameObject slowProjectile;

    //the block projectile in this scene
    public GameObject blockProjectile;

    //the stun projectile in this scene
    public GameObject stunProjectile;

    //the stun color prefab that will go over the player when stunned
    public GameObject stunColor;

    //prefabs for player class
    //the block prefab
    public GameObject block;
    //the player prefab
    public GameObject player;

    //Lists of the objects that stay on field, used for saving the game state
    public List<GameObject> blocks = new List<GameObject>();
    public List<GameObject> slowProjectiles = new List<GameObject>();

    //gameobject holding all player statuses
    public GameObject sidebar;
    //prefab for the player status
    public GameObject playerStatus;
    public List<GameObject> playerStatusList = new List<GameObject>();
    //list of player icons in the status list
    List<int> completedPlayers = new List<int>();
    //prefab for the arrow showing who's turn it is
    public GameObject arrowPrefab;
    [HideInInspector]
    public GameObject arrowObject;

	//The empty gameobject that will hold all of the winner texts
	public GameObject winnerTextsHolder;
	//Winner text prefabs
	public GameObject[] winners;

    //Prefabs for all the possible pickups
    public GameObject[] pickupPrefabs;
    //The actual spawned pickups
    public List<GameObject> pickups = new List<GameObject>();

	//Amount of next turns left in the queue
	int nextTurnsLeft = 0;

    // The time the game started
    float startTime;
    //Text that has the timer
    public Text timerText;

    //Button to save the game
    public GameObject saveGameButton;
    //True when there are no animations for the next turn runnning
    public bool saveGameUsable = true;
    //True when a save game needs to be done when it is usable again
    public bool saveGameQueued = false;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogError("Two GameControllers open in one scene");
            Destroy(this);
        }

        startTime = Time.time;

        gameId = System.Guid.NewGuid().ToString();

        List<Vector3> positionsChosen = new List<Vector3>();
        //create players based on settings specified in the main menu
        for (int i = 0; i < GameSettings.players; i++) {
            for (int s = 0; s < GameSettings.units; s++) {
                GameObject newPlayer = Instantiate(player);

                Player playerScript = newPlayer.GetComponent<Player>();

                playerScript.playerNum = i;
                playerScript.idleColor = playerColors[i];
                playerScript.selected = false;
                if (s == 0) playerScript.selected = true;

                Vector3 position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));

                while (positionsChosen.Contains(position)) {
                    print("fixing");
                    position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));
                }

                newPlayer.transform.position = position;

                players.Add(newPlayer);
                positionsChosen.Add(position);
            }
        }

        personAmount = GameSettings.players;

        usablePlayers = new bool[personAmount];
        for (int i = 0; i < usablePlayers.Length; i++) {
            usablePlayers[i] = true;
        }

        if (GameSettings.gameToLoad != -1) {
            LoadGame();
        } else if (GameSettings.connectedServer != null) { //check if this is connected to a server, and load the data if so

            //each time a new playernum (person) is spawned it is added to this list
            List<int> playerNumSpawned = new List<int>();

            foreach (PlayerData playerData in GameSettings.serverPlayerData) {
                GameObject newPlayer = players[GameSettings.serverPlayerData.IndexOf(playerData)];

                Player playerScript = newPlayer.GetComponent<Player>();

                playerScript.playerNum = playerData.playerNum;
                playerScript.idleColor = playerColors[playerScript.playerNum];
                playerScript.selected = false;
                if (!playerNumSpawned.Contains(playerScript.playerNum)) playerScript.selected = true;

                newPlayer.transform.position = playerData.position;

                if (!playerNumSpawned.Contains(playerScript.playerNum)) {
                    playerNumSpawned.Add(playerScript.playerNum);
                }
            }

            foreach (PickupData pickupData in GameSettings.serverPickupData) {
                print(pickupData.type);
                GameObject newPickup = Instantiate(pickupPrefabs[pickupData.type]);

                newPickup.transform.position = pickupData.position;

                pickups.Add(newPickup);
            }

            CreatePlayerStatus();

        } else {

            List<Vector3> pickupPositionsChosen = new List<Vector3>();

            for (int i = 0; i < (int)(personAmount * 0.25f) + 3; i++) {
                GameObject newPickup = Instantiate(pickupPrefabs[(int)Random.Range(0f, pickupPrefabs.Length)]);

                Vector3 position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));

                while (pickupPositionsChosen.Contains(position) || positionsChosen.Contains(position)) {
                    print("fixing");

                    //try again until it is not an already chosen position too
                    position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));
                }

                newPickup.transform.position = position;

                pickups.Add(newPickup);
                pickupPositionsChosen.Add(position);
            }

            CreatePlayerStatus();
        }

        if (GameSettings.serverSocket != null) { //check if hosting a server, if so then send out he player positions
            foreach (GameObject player in players) {
                Player playerScript = player.GetComponent<Player>();

                GameSettings.SendToAll("player: {" + players.IndexOf(player) + "} {" + player.transform.position.x + "} {" + player.transform.position.y + "} {" + playerScript.playerNum + "};");
            }

            foreach (GameObject pickup in pickups) {
                Pickup pickupScript = pickup.GetComponent<Pickup>();

                GameSettings.SendToAll("pickup: {" + pickups.IndexOf(pickup) + "} {" + pickup.transform.position.x + "} {" + pickup.transform.position.y + "} {" + pickupScript.type + "};");
            }

            GameSettings.SendToAll("start;");

        }
    }
	
	void FixedUpdate () {
        //Draw to turn timer
        turnNumText.text = "Turn " + (turnNum + 1);

        //Draw to timer

        float elapsedTime = Time.time - startTime;

        string time = "";

        if (elapsedTime >= 3600) { //over an hour
            int hoursTaken = ((int)(elapsedTime / 3600));

            //add a 0 if it is less than ten to look like a clock
            if (hoursTaken < 10) {
                time += "0" + hoursTaken + ":";
            } else {
                time += hoursTaken + ":";
            }
        }

        if (elapsedTime >= 60) { //over a minute
            //subtract the hours taken
            int minutesTaken = ((int)(elapsedTime / 60 - (int)(elapsedTime / 3600) * 60));

            //add a 0 if it is less than ten to look like a clock
            if (minutesTaken < 10) {
                time += "0" + minutesTaken + ":";
            } else {
                time += minutesTaken + ":";
            }
        }

        //subtract the hours taken and minutes taken
        int secondsTaken = ((int)(elapsedTime - (int)(elapsedTime / 60) * 60));

        //add a 0 if it is less than ten to look like a clock
        if (secondsTaken < 10) {
            time += "0" + secondsTaken;
        } else {
            time += secondsTaken;
        }

        timerText.text = "Time: " + time;

        //Check next turn queue and call next turn if nessesary
        if (nextTurnsLeft > 0 && !arrowObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("movement")) {
            nextTurnsLeft--;
            NextTurn();
        }

        //Check to see if there is a save game queued
        if(saveGameQueued && saveGameUsable) {
            SaveGame();
            saveGameQueued = false;
        }
    }

    public void NextTurn() {
		if (arrowObject.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("movement")) {
			nextTurnsLeft++;
			return;
		}

		do {
			turnPlayerNum++;

			if (turnPlayerNum >= personAmount) {
				turnNum++;
				turnPlayerNum = 0;
				arrowObject.GetComponent<AnimationScript> ().direction = 90;

                //if it is a new turn then spawn a new pickup

                //spawn new pickup if there aren't already the maximum
                if (pickups.Count < 7) {
                    List<Vector3> positionsChosen = new List<Vector3>();

                    //add all the currently occupied positions to the list
                    foreach (GameObject player in players) {
                        positionsChosen.Add(player.transform.position);
                    }
                    foreach (GameObject pickup in pickups) {
                        positionsChosen.Add(pickup.transform.position);
                    }

                    GameObject newPickup = Instantiate(pickupPrefabs[(int)Random.Range(0f, pickupPrefabs.Length)]);

                    Vector3 position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));

                    while (positionsChosen.Contains(position)) {
                        print("fixing");
                        position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));
                    }

                    newPickup.transform.position = position;

                    pickups.Add(newPickup);
                }

            } else {
				arrowObject.GetComponent<AnimationScript> ().direction = 270;
			}
		} while (int.Parse (GameController.instance.playerStatusList [turnPlayerNum].GetComponentInChildren<Text> ().text) == 0 || !usablePlayers[turnPlayerNum]);

		arrowObject.GetComponent<AnimationScript>().target = arrowObject.transform.parent.position + new Vector3(-1f, -(completedPlayers.IndexOf(turnPlayerNum) * 1.3f));

        arrowObject.GetComponent<Animator>().SetTrigger("move");

        lastMove = Time.time;
    }

    public void LoadGame() {
        gameId = PlayerPrefs.GetString("Game" + GameSettings.gameToLoad + "GameId");

        personAmount = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "PersonAmount");

        usablePlayers = new bool[personAmount];
        for(int i = 0; i < usablePlayers.Length; i++) {
            usablePlayers[i] = false;
        }

        turnNum = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "TurnNumber");
        turnPlayerNum = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "TurnPlayerNumber");

        startTime = Time.time - PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Time");

        int aliveUnitsAmount = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "AliveUnitsAmount");

        for (int i = 0; i < aliveUnitsAmount; i++) {
            GameObject newPlayer = Instantiate(player);

            Player playerScript = newPlayer.GetComponent<Player>();

            playerScript.playerNum = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "Player" + i + "PlayerNum");

            float r = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "IdleR");
            float g = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "IdleG");
            float b = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "IdleB");
            playerScript.idleColor = new Color(r, g, b);

            r = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "HighlightR");
            g = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "HighlightG");
            b = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "HighlightB");
            playerScript.highlightColor = new Color(r, g, b);

            r = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "ShootR");
            g = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "ShootG");
            b = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "ShootB");
            playerScript.shootColor = new Color(r, g, b);

            playerScript.selected = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "Player" + i + "Selected") == 1;

            float x = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "X");
            float y = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Player" + i + "Y");

            newPlayer.transform.position = new Vector3(x, y);

            if(PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "Player" + i + "HoldingPickup") == 1) {
                playerScript.pickup = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "Player" + i + "Pickup");
            }

            playerScript.stunned = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "Player" + i + "Stunned") == 1;
            playerScript.turnStunned = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "Player" + i + "TurnsStunned");
            if (playerScript.stunned) {
                if (turnPlayerNum > playerScript.playerNum) {
                    playerScript.turnStunned++;
                }

                playerScript.stunnedColor = Instantiate(stunColor);
                playerScript.stunnedColor.transform.position = newPlayer.transform.position;

            } else {
                usablePlayers[playerScript.playerNum] = true;
            }

            

            players.Add(newPlayer);
        }

        int pickupAmount = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "PickupAmount");

        for (int i = 0; i < pickupAmount; i++) {
            GameObject newPickup = Instantiate(pickupPrefabs[PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "Pickup" + i + "Type")]);

            float x = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Pickup" + i + "X");
            float y = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Pickup" + i + "Y");

            Vector3 position = new Vector3(x, y);

            newPickup.transform.position = position;

            pickups.Add(newPickup);

        }

        int blockAmount = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "BlockAmount");

        for (int i = 0; i < blockAmount; i++) {
            GameObject newBlock = Instantiate(block);

            float x = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Block" + i + "X");
            float y = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "Block" + i + "Y");

            Vector3 position = new Vector3(x, y);

            newBlock.transform.position = position;

            blocks.Add(newBlock);

        }

        int slowProjectileAmount = PlayerPrefs.GetInt("Game" + GameSettings.gameToLoad + "SlowProjectileAmount");

        for (int i = 0; i < blockAmount; i++) {
            GameObject newSlowProjectile = Instantiate(slowProjectile);

            float x = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "SlowProjectile" + i + "X");
            float y = PlayerPrefs.GetFloat("Game" + GameSettings.gameToLoad + "SlowProjectile" + i + "Y");

            Vector3 position = new Vector3(x, y);

            newSlowProjectile.transform.position = position;

            slowProjectiles.Add(newSlowProjectile);

        }

        CreatePlayerStatus();

    }

    public void SaveGame() {

        if (!saveGameUsable) {
            saveGameQueued = true;
            return;
        }

        int gameIndex = 0;

        if (PlayerPrefs.HasKey("GameAmount")) {
            gameIndex = PlayerPrefs.GetInt("GameAmount");

            for(int i = 0; i < gameIndex; i++) {
                if(PlayerPrefs.GetString("Game" + i + "GameId") == gameId) {
                    gameIndex = i;
                    break;
                }
            }
        }

        PlayerPrefs.SetString("Game" + gameIndex + "GameId", gameId);

        PlayerPrefs.SetInt("Game" + gameIndex + "PersonAmount", personAmount);

        PlayerPrefs.SetInt("Game" + gameIndex + "TurnNumber", turnNum);

        PlayerPrefs.SetInt("Game" + gameIndex + "TurnPlayerNum", turnPlayerNum);

        PlayerPrefs.SetFloat("Game" + gameIndex + "Time", Time.time - startTime);

        PlayerPrefs.SetInt("Game" + gameIndex + "AliveUnitsAmount", players.Count);

        for (int i = 0; i < players.Count; i++) {
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "X", players[i].transform.position.x);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "Y", players[i].transform.position.y);

            Player player = players[i].GetComponent<Player>();

            PlayerPrefs.SetInt("Game" + gameIndex + "Player" + i + "PlayerNum", player.playerNum);

            PlayerPrefs.SetInt("Game" + gameIndex + "Player" + i + "HoldingPickup", player.holding ? 1 : 0);

            if (player.holding) {
                PlayerPrefs.SetInt("Game" + gameIndex + "Player" + i + "Pickup", player.pickup);
            }

            PlayerPrefs.SetInt("Game" + gameIndex + "Player" + i + "Stunned", player.stunned ? 1 : 0);
            PlayerPrefs.SetInt("Game" + gameIndex + "Player" + i + "TurnsStunned", player.turnStunned);

            PlayerPrefs.SetInt("Game" + gameIndex + "Player" + i + "Selected", player.selected ? 1 : 0);

            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "IdleR", player.idleColor.r);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "IdleG", player.idleColor.g);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "IdleB", player.idleColor.b);

            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "HighlightR", player.highlightColor.r);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "HighlightG", player.highlightColor.g);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "HighlightB", player.highlightColor.b);

            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "ShootR", player.shootColor.r);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "ShootG", player.shootColor.g);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Player" + i + "ShootB", player.shootColor.b);

        }

        PlayerPrefs.SetInt("Game" + gameIndex + "PickupAmount", pickups.Count);

        for (int i = 0; i < pickups.Count; i++) {
            PlayerPrefs.SetFloat("Game" + gameIndex + "Pickup" + i + "X", pickups[i].transform.position.x);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Pickup" + i + "Y", pickups[i].transform.position.y);

            Pickup pickup = pickups[i].GetComponent<Pickup>();

            PlayerPrefs.SetInt("Game" + gameIndex + "Pickup" + i + "Type", pickup.type);

        }

        PlayerPrefs.SetInt("Game" + gameIndex + "BlockAmount", blocks.Count);

        for (int i = 0; i < blocks.Count; i++) {
            PlayerPrefs.SetFloat("Game" + gameIndex + "Block" + i + "X", blocks[i].transform.position.x);
            PlayerPrefs.SetFloat("Game" + gameIndex + "Block" + i + "Y", blocks[i].transform.position.y);
        }

        PlayerPrefs.SetInt("Game" + gameIndex + "SlowProjectileAmount", slowProjectiles.Count);

        for (int i = 0; i < slowProjectiles.Count; i++) {
            PlayerPrefs.SetFloat("Game" + gameIndex + "SlowProjectile" + i + "X", slowProjectiles[i].transform.position.x);
            PlayerPrefs.SetFloat("Game" + gameIndex + "SlowProjectile" + i + "Y", slowProjectiles[i].transform.position.y);
        }

        if (gameIndex == PlayerPrefs.GetInt("GameAmount")) {
            PlayerPrefs.SetInt("GameAmount", gameIndex + 1);
        }

    }

    void CreatePlayerStatus() {
        //create arrow pointing at who's turn it is
        arrowObject = Instantiate(arrowPrefab);
        arrowObject.transform.parent = sidebar.transform;

        //create player status'
        foreach (GameObject player in players) {
            Player playerScript = player.GetComponent<Player>();

            if (!completedPlayers.Contains(playerScript.playerNum)) {
                GameObject newPlayerStatus = Instantiate(playerStatus);
                newPlayerStatus.transform.parent = sidebar.transform;
                newPlayerStatus.transform.localPosition = new Vector3(0, -(completedPlayers.Count * 1.3f));
                newPlayerStatus.GetComponent<SpriteRenderer>().color = playerScript.idleColor;

                if (turnPlayerNum == playerScript.playerNum) {
                    arrowObject.transform.localPosition = new Vector3(-1f, -(completedPlayers.Count * 1.3f));
                }

                completedPlayers.Add(playerScript.playerNum);
                playerStatusList.Add(newPlayerStatus);
            } else {
                playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text = int.Parse(playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text) + 1 + "";
            }

        }

        //completedPlayers = new List<int>();
    }

    //disconnect all the sockets when the program is exiting
    void OnApplicationQuit() {
        GameSettings.OnApplicationQuit();
    }
}
