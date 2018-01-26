using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;


public class HostingScript : MonoBehaviour {

    Thread t;

    //Prefab for the player Text (spawned in to show the player everyone who has connected)
    public GameObject playerTextPrefab;

    //The object that all the player texts will be children of
    public GameObject playerTextParent;

    //List of all the player texts
    public List<GameObject > playerTexts = new List<GameObject>();

    //Set from second thread so that the main thread can spawn them
    public int playersToSpawn = 0;

    //Called by a thread to make the player text removed on the main thread
    public List<ConnectedSocket> playersToRemove = new List<ConnectedSocket>();

    //The startUnits selector
    public Text startUnits;

    //The unit amount chooser to change the maximum
    public AmountChooser unitAmountChooser;

    //Amount of players connected including the host
    public int playerAmount = 1;

    void Start () {

        GameSettings.serverSocket = new TcpListener(IPAddress.Any, 1273);

        GameSettings.serverSocket.Start();

        t = new Thread(new ThreadStart(WaitForConnection));
        t.Start();

    }

    void Update () {
		for(int i = 0; i < playersToSpawn; i++) {
            AddPlayerToList();
        }
        playersToSpawn = 0;

        for (int i = 0; i < playersToRemove.Count;) {

            RemovePlayerFromList(playersToRemove[i]);

            playersToRemove.RemoveAt(i);
        }
	}

    public void StartGame() {
        if (t != null) {
            t.Abort();
        }

        GameSettings.players = GameSettings.connectedPlayers.Count + 1;
        GameSettings.units = int.Parse(startUnits.text);

        foreach (ConnectedSocket connectedSocket in GameSettings.connectedPlayers) {
            connectedSocket.SendMessage("players: {" + GameSettings.players + "};");
            connectedSocket.SendMessage("units: {" + GameSettings.units + "};");
            connectedSocket.SendMessage("current: {" + (GameSettings.connectedPlayers.IndexOf(connectedSocket) + 1) + "};");
        }

        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void AddPlayerToList() {
        playerAmount++;
        unitAmountChooser.playerAmount = playerAmount;

        GameObject playerText = Instantiate(playerTextPrefab);

        playerText.GetComponent<Text>().text = "Player " + (playerTexts.Count + 2);

        playerText.transform.SetParent(playerTextParent.transform);

        RectTransform playerTextTransform = playerText.GetComponent<RectTransform>();
        playerTextTransform.anchoredPosition = new Vector2(0, 190 - ((playerTexts.Count + 1) * 30));
        playerTextTransform.localScale = Vector2.one;

        playerTexts.Add(playerText);
    }

    public void RemovePlayerFromList(ConnectedSocket client) {
        playerAmount--;
        unitAmountChooser.playerAmount = playerAmount;

        int index = GameSettings.connectedPlayers.IndexOf(client);

        GameObject playerText = null;

        if (playerTexts.Count <= index) {
            playersToSpawn--;
        } else {
            playerText = playerTexts[index];

            for (int i = index; i < playerTexts.Count; i++) {
                playerTexts[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 30);

                playerTexts[i].GetComponent<Text>().text = "Player " + (i + 1); //plus one instead because the first player isn't in the array (it's the this computer)
            }


            Destroy(playerText);

            playerTexts.RemoveAt(index);

        }

        GameSettings.connectedPlayers.Remove(client);
    }

    public void WaitForConnection() {
        TcpListener serverSocket = GameSettings.serverSocket;

        TcpClient clientSocket = serverSocket.AcceptTcpClient();
        print("connected");

        //NetworkStream networkStream = clientSocket.GetStream();
        //byte[] bytesFrom = new byte[50];
        //networkStream.Read(bytesFrom, 0, bytesFrom.Length);

        //print(System.Text.Encoding.ASCII.GetString(bytesFrom));

        ConnectedSocket connectedPlayer = new ConnectedSocket(clientSocket, null);

        GameSettings.connectedPlayers.Add(connectedPlayer);

        //this will make the main thread spawn a new player text
        playersToSpawn++;

        t = new Thread(new ThreadStart(WaitForConnection));
        t.Start();

        Thread disconnectThread = new Thread(new ThreadStart(() => connectedPlayer.WaitForDisconnect(playersToRemove)));
        disconnectThread.Start();

        connectedPlayer.playerDisconnectThread = disconnectThread;

        Thread messageThread = new Thread(connectedPlayer.WaitForMessages);
        messageThread.Start();

        connectedPlayer.playerMessageThread = messageThread;
    }

    void OnApplicationQuit() {

        GameSettings.OnApplicationQuit();

        if (t != null) {
            t.Abort();
        }
    }

}
