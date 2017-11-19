using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public static GameController instance = null;

    GameObject[] players = new GameObject[0];

    int turnNum = 0;

    ///<summary>
    /// What player's turn is it
    ///</summary>
    int playerTurn = 0;

	void Start () {
        if(instance == null) {
            GameController.instance = this;
        }else {
            Debug.LogError("Two GameControllers open in one scene");
            Destroy(this);
        }
	}
	
	void Update () {
		
	}
}
