using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;  // 비동기 처리

public class ClientSocket : MonoBehaviour
{
    private TcpClient socket;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public string Host = "192.168.137.199";  // Python 서버 IP
    public int Port = 1234;  // 포트 번호

    [SerializeField] private int stressLevel = 0;  // Inspector와 동기화
    public int StressLevel
    {
        get => stressLevel;
        private set
        {
            stressLevel = value;
            Debug.Log($"Updated StressLevel in Unity: {stressLevel}");
        }
    }

    public bool TurnonOutput { get; set; } = false;  // TurnonOutput 값
    private bool isReceiving = false;

    void Start()
    {
        ConnectToServer();
    }

    void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

    private async void ConnectToServer()
    {
        try
        {
            socket = new TcpClient(Host, Port);
            stream = socket.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            Debug.Log("Connected to Python server.");

            // Start receiving StressLevel values asynchronously
            isReceiving = true;
            await ReceiveStressLevelAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection error: {e.Message}");
        }
    }

    private void DisconnectFromServer()
    {
        isReceiving = false;
        if (socket != null)
        {
            socket.Close();
        }
        Debug.Log("Disconnected from Python server.");
    }

    private async Task ReceiveStressLevelAsync()
    {
        while (isReceiving)
        {
            try
            {
                if (reader != null)
                {
                    string received = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(received))
                    {
                        var data = JsonUtility.FromJson<StressLevelData>(received);
                        if (data != null)
                        {
                            StressLevel = data.stresslevel;  // Update StressLevel
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error receiving StressLevel: {e.Message}");
                break;  // Exit the loop if there's an error
            }

            await Task.Delay(1000);  // 초당 업데이트
        }
    }

    public async void SendTurnonOutputAsync()
    {
        if (socket != null && socket.Connected)
        {
            try
            {
                var message = new TurnonOutputData { turnonoutput = TurnonOutput ? 1 : 0 };
                string jsonMessage = JsonUtility.ToJson(message) + "\n"; // Add newline for Python compatibility
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
