using System.Collections;
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

	//Amount of next turns left in the queue
	int nextTurnsLeft = 0;

    void Awake () {
        if(instance == null) {
            instance = this;
        }else {
            Debug.LogError("Two GameControllers open in one scene");
            Destroy(this);
        }

        List<Vector3> positionsChosen = new List<Vector3>();
        //create players based on settings specified in the main menu
        for(int i = 0; i < GameSettings.players; i++) {
            for(int s = 0; s < GameSettings.units; s++) {
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
	}
	
	void FixedUpdate () {
        turnNumText.text = "Turn " + (turnNum + 1);

		if (nextTurnsLeft > 0 && !arrowObject.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("movement")) {
			nextTurnsLeft--;
			NextTurn ();
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
