using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class HostingScript : MonoBehaviour {

	void Start () {

        GameSettings.serverSocket = new TcpListener(IPAddress.Any, 1273);

        GameSettings.serverSocket.Start();

        Thread t = new Thread(new ThreadStart(WaitForConnection));
        t.Start();

    }

    void Update () {
		
	}

    public void WaitForConnection() {
        TcpListener serverSocket = GameSettings.serverSocket;

        TcpClient clientSocket = serverSocket.AcceptTcpClient();
        print("connected");

        Thread t = new Thread(new ThreadStart(WaitForConnection));
        t.Start();
    }
}
