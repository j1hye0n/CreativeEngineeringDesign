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
    public String Host = "192.168.137.99"; // Replace with the server's IP
    public Int32 Port = 12345;

    public int StressLevel = 0;  // The variable to update

    void Start()
    {
        setupSocket();
        if (socketReady)
        {
            Debug.Log("Socket set up");

            // Start listening for StressLevelInput updates
            StartCoroutine(ReceiveStressLevel());
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

    private System.Collections.IEnumerator ReceiveStressLevel()
    {
        while (socketReady)
        {
            try
            {
                // Read StressLevelInput from the server
                Byte[] readBytes = new byte[1024];
                int numberOfBytesRead = theStream.Read(readBytes, 0, readBytes.Length);
                string response = Encoding.ASCII.GetString(readBytes, 0, numberOfBytesRead);

                if (int.TryParse(response, out int stressLevelInput))
                {
                    StressLevel = stressLevelInput;  // Update StressLevel
                    Debug.Log($"Updated StressLevel: {StressLevel}");
                }
                else
                {
                    Debug.LogWarning("Failed to parse StressLevelInput from server.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error receiving StressLevelInput: " + e.Message);
            }

            // Wait for 1 second before the next read
            yield return new WaitForSeconds(1f);
        }
    }
}
