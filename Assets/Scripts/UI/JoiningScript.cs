using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;

public class JoiningScript : MonoBehaviour {

	void Start () {
        ConnectedSocket connectedServer = new ConnectedSocket(new TcpClient(), null);

        GameSettings.connectedServer = connectedServer;

        connectedServer.clientSocket.Connect("127.0.0.1", 1273);

        //setup threads

        Thread disconnectThread = new Thread(new ThreadStart(() => connectedServer.WaitForDisconnect()));
        disconnectThread.Start();

        connectedServer.playerDisconnectThread = disconnectThread;

        Thread messageThread = new Thread(connectedServer.WaitForMessages);
        messageThread.Start();

        connectedServer.playerMessageThread = messageThread;
    }
	
	void Update () {
		
	}
}
