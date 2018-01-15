﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController instance = null;

    //contains all players, including if a player has multiple characters on screen
    public List<GameObject> players = new List<GameObject>();

	// Amount of actual people playing/turns to go through. Because the players array now includes more than one player per person
    public int personAmount = 0;

	//Amount of players with no units left
	public int playersDead = 0;

	//stores all the player nums in the order they died (the last player class that was left
	public List<Player> playersDeadList = new List<Player>();

	//if the game is over and it is showing the scoreboard
	public bool gameOver = false;

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

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogError("Two GameControllers open in one scene");
            Destroy(this);
        }

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

        List<Vector3> pickupPositionsChosen = new List<Vector3>();

        for (int i = 0; i < (int)(personAmount * 0.25f) + 3; i++) {
            GameObject newPickup = Instantiate(pickupPrefabs[(int)Random.Range(0f, pickupPrefabs.Length)]);

            Vector3 position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));

            while (pickupPositionsChosen.Contains(position) || positionsChosen.Contains(position)) {
                print("fixing");
                position = new Vector3(Mathf.RoundToInt(Random.Range(-9f, 9f)), Mathf.RoundToInt(Random.Range(-9f, 9f)));
            }

            newPickup.transform.position = position;

            pickups.Add(newPickup);
            pickupPositionsChosen.Add(position);
        }

        //create arrow pointing at who's turn it is
        arrowObject = Instantiate(arrowPrefab);
        arrowObject.transform.parent = sidebar.transform;

        //create player status'
        foreach(GameObject player in players) {
            Player playerScript = player.GetComponent<Player>();

            if (!completedPlayers.Contains(playerScript.playerNum)) {
                GameObject newPlayerStatus = Instantiate(playerStatus);
                newPlayerStatus.transform.parent = sidebar.transform;
                newPlayerStatus.transform.localPosition = new Vector3(0, - (completedPlayers.Count * 1.3f));
                newPlayerStatus.GetComponent<SpriteRenderer>().color = playerScript.idleColor;

                if(turnPlayerNum == playerScript.playerNum) {
                    arrowObject.transform.localPosition = new Vector3(-1f, -(completedPlayers.Count * 1.3f));
                }

                completedPlayers.Add(playerScript.playerNum);
                playerStatusList.Add(newPlayerStatus);
            } else {
                playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text = int.Parse(playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text) + 1 + "";
            }

        }

        startTime = Time.time;
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
			} else {
				arrowObject.GetComponent<AnimationScript> ().direction = 270;
			}
		} while (int.Parse (GameController.instance.playerStatusList [turnPlayerNum].GetComponentInChildren<Text> ().text) == 0);

		arrowObject.GetComponent<AnimationScript>().target = arrowObject.transform.parent.position + new Vector3(-1f, -(completedPlayers.IndexOf(turnPlayerNum) * 1.3f));

        arrowObject.GetComponent<Animator>().SetTrigger("move");

        //arrowObject.transform.localPosition = new Vector3(-1f, -(completedPlayers.IndexOf(turnPlayerNum) * 1.3f));

        lastMove = Time.time;
    }
}
