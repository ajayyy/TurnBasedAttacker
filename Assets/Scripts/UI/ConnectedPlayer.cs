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
    List<string> messageBuffer = new List<string>();

    public ConnectedPlayer(TcpClient clientSocket, Thread playerDisconnectThread) {
        this.clientSocket = clientSocket;
        this.playerDisconnectThread = playerDisconnectThread;
    }

    public void WaitForMessages() {

        while (true) {
            NetworkStream networkStream = clientSocket.GetStream();

            byte[] bytesFrom = new byte[50];
            networkStream.Read(bytesFrom, 0, bytesFrom.Length);

            messageBuffer.Add(System.Text.Encoding.ASCII.GetString(bytesFrom).Replace("\n",""));

            //Debug.Log(System.Text.Encoding.ASCII.GetString(bytesFrom));

        }

    }

    public string GetMessage() {

        if(messageBuffer.Count == 0) {
            return null;
        }

        string message = messageBuffer[0];

        return message;
    }

    //if it meant something, it can be removed from the queue
    public void RemoveMessage() {
        messageBuffer.RemoveAt(0);
    }

}
