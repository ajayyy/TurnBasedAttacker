using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using UnityEngine;

public class ConnectedPlayer {

    //stores the client socket
    public TcpClient clientSocket;

    //the thread that waits for the player to disconnect
    public Thread playerDisconnectThread;

    //the thread that waits for the player to send a message
    public Thread playerMessageThread;

    //the last messages sent since GetMessage() has been called
    string messageBuffer = "";

    public ConnectedPlayer(TcpClient clientSocket, Thread playerDisconnectThread) {
        this.clientSocket = clientSocket;
        this.playerDisconnectThread = playerDisconnectThread;
    }

    public void WaitForMessages() {

        while (true) {
            NetworkStream networkStream = clientSocket.GetStream();

            byte[] bytesFrom = new byte[50];
            networkStream.Read(bytesFrom, 0, bytesFrom.Length);

            messageBuffer += System.Text.Encoding.ASCII.GetString(bytesFrom);

        }

    }

}
