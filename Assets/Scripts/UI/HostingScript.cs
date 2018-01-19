using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
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

    void Start () {

        GameSettings.serverSocket = new TcpListener(IPAddress.Any, 1273);

        GameSettings.serverSocket.Start();

        t = new Thread(new ThreadStart(WaitForConnection));
        t.Start();

    }

    void Update () {
		for(int i = 0; i < playersToSpawn; i++) {
            GameObject playerText = Instantiate(playerTextPrefab);

            playerText.GetComponent<Text>().text = "Player " + (playerTexts.Count + 2);

            playerText.transform.SetParent(playerTextParent.transform);

            RectTransform playerTextTransform = playerText.GetComponent<RectTransform>();
            playerTextTransform.anchoredPosition = new Vector2(0, 190 - ((playerTexts.Count + 1) * 30));
            playerTextTransform.localScale = Vector2.one;

            playerTexts.Add(playerText);
        }
        playersToSpawn = 0;
	}

    public void WaitForConnection() {
        TcpListener serverSocket = GameSettings.serverSocket;

        TcpClient clientSocket = serverSocket.AcceptTcpClient();
        print("connected");

        NetworkStream networkStream = clientSocket.GetStream();
        byte[] bytesFrom = new byte[50];
        networkStream.Read(bytesFrom, 0, bytesFrom.Length);

        print(System.Text.Encoding.ASCII.GetString(bytesFrom));

        GameSettings.clientSockets.Add(clientSocket);

        //this will make the main thread spawn a new player text
        playersToSpawn++;

        t = new Thread(new ThreadStart(WaitForConnection));
        t.Start();
    }

    void OnApplicationQuit() {

        foreach(TcpClient clientSocket in GameSettings.clientSockets) {
            if (clientSocket != null) {
                clientSocket.Close();
            }
        }

        if (GameSettings.serverSocket != null) {
            GameSettings.serverSocket.Stop();
        }

        if (t != null) {
            t.Abort();
        }
    }
}
