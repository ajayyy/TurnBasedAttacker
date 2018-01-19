using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class HostingScript : MonoBehaviour {

	void Start () {

        GameSettings.serverSocket = new TcpListener(IPAddress.Any, 1273);

        TcpListener serverSocket = GameSettings.serverSocket;

        serverSocket.Start();

        TcpClient clientSocket = serverSocket.AcceptTcpClient();
        print("connected");

    }

    void Update () {
		
	}
}
