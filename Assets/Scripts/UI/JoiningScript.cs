using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;

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
        if(Time.time % 2 == 0)
            GameSettings.connectedServer.SendMessage("tessst");
	}

    void OnApplicationQuit() {
        GameSettings.OnApplicationQuit();
    }
}
