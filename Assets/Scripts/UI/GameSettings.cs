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

    //stores the server socket if this is connected to a server
    public static TcpListener serverSocket;

    //a list of all the players connected if this is connected to a server or hosting a server
    public static List<ConnectedPlayer> connectedPlayers = new List<ConnectedPlayer>();

}
