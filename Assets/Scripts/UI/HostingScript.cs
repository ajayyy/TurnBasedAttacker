using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class HostingScript : MonoBehaviour {

    Thread t;

    void Start () {

        GameSettings.serverSocket = new TcpListener(IPAddress.Any, 1273);

        GameSettings.serverSocket.Start();

        t = new Thread(new ThreadStart(WaitForConnection));
        t.Start();

    }

    void Update () {
		
	}

    public void WaitForConnection() {
        TcpListener serverSocket = GameSettings.serverSocket;

        TcpClient clientSocket = serverSocket.AcceptTcpClient();
        print("connected");

        //NetworkStream networkStream = clientSocket.GetStream();
        //byte[] bytesFrom = new byte[10025];
        //networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);

        NetworkStream networkStream = clientSocket.GetStream();
        byte[] bytesFrom = new byte[50];
        networkStream.Read(bytesFrom, 0, 50);

        print(System.Text.Encoding.ASCII.GetString(bytesFrom));

        //t = new Thread(new ThreadStart(WaitForConnection));
        //t.Start();
    }

    void OnApplicationQuit() {
        if(t != null) {
            t.Abort();
        }
    }
}
