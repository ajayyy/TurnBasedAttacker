using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class JoiningScript : MonoBehaviour {

    //the ip address text field
    public InputField ipAddress;

    //the text that shows the status of the connection
    public Text status;

    public void Connect() {
        status.text = "Error connecting to that ip";

        TcpClient clientSocket = new TcpClient();

        clientSocket.Connect(ipAddress.text, 1273);

        ConnectedSocket connectedServer = new ConnectedSocket(clientSocket, null);

        GameSettings.connectedServer = connectedServer;

        //setup threads

        Thread disconnectThread = new Thread(new ThreadStart(() => connectedServer.WaitForDisconnect()));
        disconnectThread.Start();

        connectedServer.playerDisconnectThread = disconnectThread;

        Thread messageThread = new Thread(connectedServer.WaitForMessages);
        messageThread.Start();

        connectedServer.playerMessageThread = messageThread;

        status.text = "Connected!";
        status.color = new Color(0, 1, 0);
    }

    void FixedUpdate () {
        //if(Time.time % 2 == 0)
        //    GameSettings.connectedServer.SendMessage("tessst");

        if(GameSettings.connectedServer == null) {
            return;
        }

        string message = GameSettings.connectedServer.GetMessage();

        if (message != null && message.Contains("start")) {
            GameSettings.connectedServer.RemoveMessage();

            //start game
            //GameSettings.players = GameSettings.connectedPlayers.Count + 1;
            //GameSettings.units = int.Parse(startUnits.text);

            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        if (message != null && message.Contains("players: ")) {
            GameSettings.connectedServer.RemoveMessage();

            GameSettings.players = int.Parse(message.Split('{')[1].Split('}')[0]);
        }

        if (message != null && message.Contains("units: ")) {
            GameSettings.connectedServer.RemoveMessage();

            GameSettings.units = int.Parse(message.Split('{')[1].Split('}')[0]);
        }

        if (message != null && message.Contains("current: ")) {
            GameSettings.connectedServer.RemoveMessage();

            GameSettings.currentPlayerNum = int.Parse(message.Split('{')[1].Split('}')[0]);
        }

        //check if they are sending data
        if (message != null && message.Contains("player: ")) {
            GameSettings.connectedServer.RemoveMessage();

            //index is ignored since tcp preserves order of messages
            PlayerData player = new PlayerData(new Vector2(float.Parse(message.Split('{')[2].Split('}')[0]), float.Parse(message.Split('{')[3].Split('}')[0])), int.Parse(message.Split('{')[4].Split('}')[0]));

            GameSettings.serverPlayerData.Add(player);
        }
        if (message != null && message.Contains("pickup: ")) {
            GameSettings.connectedServer.RemoveMessage();

            //index is ignored since tcp preserves order of messages
            PickupData pickup = new PickupData(new Vector2(float.Parse(message.Split('{')[2].Split('}')[0]), float.Parse(message.Split('{')[3].Split('}')[0])), int.Parse(message.Split('{')[4].Split('}')[0]));

            GameSettings.serverPickupData.Add(pickup);
        }

    }

    void OnApplicationQuit() {
        GameSettings.OnApplicationQuit();
    }
}
