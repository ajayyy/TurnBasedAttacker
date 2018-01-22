using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

public class ConnectedPlayer {

    //stores the client socket
    public TcpClient clientSocket;

    //the thread that wait for the player to disconnect
    public Thread playerDisconnectThread;

    public ConnectedPlayer(TcpClient clientSocket, Thread playerDisconnectThread) {
        this.clientSocket = clientSocket;
        this.playerDisconnectThread = playerDisconnectThread;
    }

}
