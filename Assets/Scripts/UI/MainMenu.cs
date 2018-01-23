using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

	void Start () {
        GameSettings.serverSocket = null;
        GameSettings.connectedPlayers = new List<ConnectedPlayer>();
    }
	
	void Update () {
		
	}
}
