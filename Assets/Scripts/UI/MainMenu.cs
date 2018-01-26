using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

	void Start () {
        GameSettings.serverSocket = null;
        GameSettings.connectedPlayers = new List<ConnectedSocket>();

        GameSettings.connectedServer = null;
        GameSettings.currentPlayerNum = 0;
    }
	
	void Update () {
		
	}
}
