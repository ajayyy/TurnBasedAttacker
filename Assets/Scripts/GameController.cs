using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController instance = null;

    //contains all players, including if a player has multiple characters on screen
    public List<GameObject> players = new List<GameObject>();

    public float personAmount = 0; // Amount of actual people playing/turns to go through. Because the players array now includes more than one player per person

    public int turnNum = 0;

    ///<summary>
    /// What player's turn is it
    ///</summary>
    public int turnPlayerNum = 0;

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

    //prefabs for player class
    //the block prefab
    public GameObject block;
    //the player prefab
    public GameObject player;

    void Awake () {
        if(instance == null) {
            instance = this;
        }else {
            Debug.LogError("Two GameControllers open in one scene");
            Destroy(this);
        }
	}
	
	void Update () {
		
	}

    public void NextTurn() {
        turnPlayerNum++;
        if(turnPlayerNum >= personAmount) {
            turnNum++;
            turnPlayerNum = 0;
        }

        lastMove = Time.time;
    }
}
