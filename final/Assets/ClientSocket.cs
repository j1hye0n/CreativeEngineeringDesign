using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;  // For threading

public class ClientSocket : MonoBehaviour
{
    private TcpClient socket;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;  // Added writer for sending TurnonOutput
    private Thread clientThread;  // Dedicated thread for Python communication
    private bool isRunning = true;

    public string Host = "192.168.137.173";  // Python server IP
    public int Port = 1234;  // Corrected Port

    public static int StressLevel { get; private set; } = 0;  // Shared StressLevel value
    public bool TurnonOutput { get; set; } = false;  // Property for TurnonOutput

    void Start()
    {
        try
        {
            socket = new TcpClient(Host, Port);
            stream = socket.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };  // Initialize writer

            // Start a new thread for receiving data
            clientThread = new Thread(ReceiveData);
            clientThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"Socket setup error: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;

        if (clientThread != null && clientThread.IsAlive)
        {
            clientThread.Abort();
        }

        if (socket != null)
        {
            socket.Close();
        }
    }

    private void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                string received = reader.ReadLine();  // Read data from Python
                if (!string.IsNullOrEmpty(received))
                {
                    // Parse the received JSON
                    var data = JsonUtility.FromJson<StressLevelData>(received);
                    if (data != null)
                    {
                        StressLevel = data.stresslevel;  // Update StressLevel
                        Debug.Log($"Received StressLevel: {StressLevel}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error receiving data: {e.Message}");
            }

            // Avoid tight looping
            Thread.Sleep(100);
        }
    }

    public async void SendTurnonOutputAsync()
    {
        if (socket != null && socket.Connected)
        {
            try
            {
                var message = new TurnonOutputData { turnonoutput = TurnonOutput ? 1 : 0 };
                string jsonMessage = JsonUtility.ToJson(message) + "\n"; // Add \n for Python compatibility
                await writer.WriteLineAsync(jsonMessage);  // 비동기 송신
                Debug.Log($"Async Sent TurnonOutput: {message.turnonoutput}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error sending TurnonOutput asynchronously: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Socket is not connected. Cannot send TurnonOutput asynchronously.");
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
