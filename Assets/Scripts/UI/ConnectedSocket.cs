using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using UnityEngine;

public class ConnectedSocket {

    //stores the client socket
    public TcpClient clientSocket;

    //the thread that waits for the player to disconnect
    public Thread playerDisconnectThread;

    //the thread that waits for the player to send a message
    public Thread playerMessageThread;

    //the last messages sent since GetMessage() has been called
    List<string> messageBuffer = new List<string>();

    //the network stream for reading and sending messages
    NetworkStream networkStream;

    public ConnectedSocket(TcpClient clientSocket, Thread playerDisconnectThread) {
        this.clientSocket = clientSocket;
        this.playerDisconnectThread = playerDisconnectThread;

        networkStream = clientSocket.GetStream();
    }

    public void WaitForMessages() {

        while (true) {

            byte[] bytesFrom = new byte[50];
            networkStream.Read(bytesFrom, 0, bytesFrom.Length);

            messageBuffer.Add(System.Text.Encoding.ASCII.GetString(bytesFrom).Replace("\n",""));

            //Debug.Log(System.Text.Encoding.ASCII.GetString(bytesFrom));

        }

    }

    public bool WaitForDisconnect() {
        // Detect if client disconnected

        if (clientSocket.Client.Poll(0, SelectMode.SelectWrite) && !clientSocket.Client.Poll(0, SelectMode.SelectError)) {
            byte[] buff = new byte[1];

            if (clientSocket.Client.Receive(buff, SocketFlags.Peek) == 0) {
                // Client disconnected

                Debug.Log("disconnected");

                return true;

            }
        }

        return false;
    }

    public void WaitForDisconnect(List<ConnectedSocket> playersToRemove) {
        if (WaitForDisconnect()) {
            playersToRemove.Add(this);
        }
    }

    public void SendMessage(string message) {

        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(message);
        networkStream.Write(outStream, 0, outStream.Length);

        networkStream.Flush();
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
