using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

public class GameSettings {

    //number of players playing
    public static int players = 2;

    //number of units per player
    public static int units = 1;

    //the game to load, if -1 then no saved game will load
    public static int gameToLoad = -1;

    //variables if connected to a server

    //the server this game is connected to
    //not null when connected to a server, stores the socket to communicate with the server
    public static ConnectedSocket connectedServer;

    //if connected to a server, used to tell who can be controlled, and who is controlled by others
    public static int currentPlayerNum;

    //variables if hosting a server

    //stores the server socket if this is connected to a server
    public static TcpListener serverSocket;

    //a list of all the players connected if this is connected to a server or hosting a server
    public static List<ConnectedSocket> connectedPlayers = new List<ConnectedSocket>();

    //Functions

    //if hosting the server
    public static void SendToAllExcept(string message, ConnectedSocket except) {
        foreach (ConnectedSocket connectedPlayer in connectedPlayers) {
            if (connectedPlayer == except) continue;

            connectedPlayer.SendMessage(message);
        }
    }

    //called by mono behaviors to close all network connections when the game is closed
    public static void OnApplicationQuit() {

        foreach (ConnectedSocket connectedPlayer in connectedPlayers) {
            if (connectedPlayer.clientSocket != null) {
                connectedPlayer.clientSocket.Close();
            }

            if (connectedPlayer.playerDisconnectThread != null) {
                connectedPlayer.playerDisconnectThread.Abort();
            }

            if (connectedPlayer.playerMessageThread != null) {
                connectedPlayer.playerMessageThread.Abort();
            }

            if(connectedPlayer.networkStream != null) {
                connectedPlayer.networkStream.Close();
            }
        }

        if (connectedServer != null) {
            if(connectedServer.clientSocket != null) {
                connectedServer.clientSocket.Close();
            }

            if (connectedServer.playerDisconnectThread != null) {
                connectedServer.playerDisconnectThread.Abort();
            }

            if (connectedServer.playerMessageThread != null) {
                connectedServer.playerMessageThread.Abort();
            }
        }

        if (serverSocket != null) {
            serverSocket.Stop();
        }

    }

}
