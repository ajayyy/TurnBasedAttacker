using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Networking : MonoBehaviour {

    void Start() {

        TcpClient clientSocket = new TcpClient();

        TcpListener serverSocket = new TcpListener(IPAddress.Any, 1273);

        clientSocket.Connect("127.0.0.1", 1273);
        NetworkStream serverStream = clientSocket.GetStream();

        //NetworkStream serverStream = clientSocket.GetStream();
        //byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox2.Text + "$");
        //serverStream.Write(outStream, 0, outStream.Length);
        //serverStream.Flush();

        //byte[] inStream = new byte[10025];
        //serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
        //string returndata = System.Text.Encoding.ASCII.GetString(inStream);
    }

}
