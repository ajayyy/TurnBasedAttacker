using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine;

public class JoiningScript : MonoBehaviour {

	void Start () {

        TcpClient clientSocket = new TcpClient();

        clientSocket.Connect("127.0.0.1", 1273);

        ConnectedSocket connectedServer = new ConnectedSocket(clientSocket, null);

        GameSettings.connectedServer = connectedServer;

        //setup threads

        Thread disconnectThread = new Thread(new ThreadStart(() => connectedServer.WaitForDisconnect()));
        disconnectThread.Start();

        connectedServer.playerDisconnectThread = disconnectThread;

        Thread messageThread = new Thread(connectedServer.WaitForMessages);
        messageThread.Start();

        connectedServer.playerMessageThread = messageThread;
    }
	
	void FixedUpdate () {
        //if(Time.time % 2 == 0)
        //    GameSettings.connectedServer.SendMessage("tessst");

        string message = GameSettings.connectedServer.GetMessage();

        if (message != null && message.Contains("start")) {
            GameSettings.connectedServer.RemoveMessage();

            //start game
            //GameSettings.players = GameSettings.connectedPlayers.Count + 1;
            //GameSettings.units = int.Parse(startUnits.text);

            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
	}

    void OnApplicationQuit() {
        GameSettings.OnApplicationQuit();
    }
}
