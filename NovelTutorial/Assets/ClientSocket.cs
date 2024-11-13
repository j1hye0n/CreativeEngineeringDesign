using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

public class ClientSocket : MonoBehaviour
{
    private bool socketReady = false;
    private TcpClient mySocket;
    public NetworkStream theStream;
    private StreamWriter theWriter;
    private StreamReader theReader;
    public String Host = "10.101.51.38"; // Replace with the server's IP
    public Int32 Port = 12345;

    void Start()
    {
        setupSocket();
        if (socketReady)
        {
            Debug.Log("Socket set up");

            // Send message
            Byte[] sendBytes = Encoding.UTF8.GetBytes("This is the client");
            theStream.Write(sendBytes, 0, sendBytes.Length);

            Debug.Log("Sent message. Waiting for response");

            // Receive response
            Byte[] readBytes = new byte[1024];
            int numberOfBytesRead = theStream.Read(readBytes, 0, readBytes.Length);
            string response = Encoding.ASCII.GetString(readBytes, 0, numberOfBytesRead);
            Debug.Log("You received the following message: " + response);
        }
    }

    void OnApplicationQuit()
    {
        closeSocket();
    }

    private void setupSocket()
    {
        try
        {
            mySocket = new TcpClient(Host, Port);
            theStream = mySocket.GetStream();
            theWriter = new StreamWriter(theStream);
            theReader = new StreamReader(theStream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error:" + e);
        }
    }

    private void closeSocket()
    {
        if (!socketReady) return;
        theWriter.Close();
        theReader.Close();
        mySocket.Close();
        socketReady = false;
    }
}
