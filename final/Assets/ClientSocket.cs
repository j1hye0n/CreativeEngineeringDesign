using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;

public class ClientSocket : MonoBehaviour
{
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    public string Host = "192.168.137.99";  // Replace with Python server IP
    public int Port = 12345;

    public int StressLevel { get; private set; } = 0;
    public bool TurnonOutput = false;

    void Start()
    {
        SetupSocket();
        if (socket.Connected)
        {
            Debug.Log("Socket connected");
            StartCoroutine(ReceiveData());
        }
    }

    void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void SetupSocket()
    {
        try
        {
            socket = new TcpClient(Host, Port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
        }
        catch (Exception e)
        {
            Debug.LogError($"Socket error: {e.Message}");
        }
    }

    private void CloseSocket()
    {
        if (socket != null)
        {
            writer.Close();
            reader.Close();
            socket.Close();
        }
    }

    private IEnumerator ReceiveData()
    {
        while (socket.Connected)
        {
            try
            {
                // Read data from Python
                string received = reader.ReadLine();
                if (!string.IsNullOrEmpty(received))
                {
                    var data = JsonUtility.FromJson<StressLevelData>(received);
                    if (data != null)
                    {
                        StressLevel = data.stresslevel;
                        Debug.Log($"Received StressLevel: {StressLevel}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error receiving data: {e.Message}");
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void SendTurnonOutput()
    {
        if (socket.Connected)
        {
            try
            {
                var message = new TurnonOutputData { turnonoutput = TurnonOutput ? 1 : 0 };
                string jsonMessage = JsonUtility.ToJson(message);
                writer.WriteLine(jsonMessage);
                writer.Flush();
                Debug.Log($"Sent TurnonOutput: {message.turnonoutput}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error sending data: {e.Message}");
            }
        }
    }

    [Serializable]
    private class StressLevelData
    {
        public int stresslevel;
    }

    [Serializable]
    private class TurnonOutputData
    {
        public int turnonoutput;
    }
}
