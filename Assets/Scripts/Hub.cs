using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Hub : MonoBehaviour
{
    public MusicSpeaker musicSpeaker;
    public TV tv;
    
    private TcpListener tcpListener;
    private Thread serverThread;
    private bool isServerRunning = false;
    public int port = 8080;
    
    [SerializeField] private string serverIP = "Not started";
    [SerializeField] private string serverStatus = "Stopped";
    
    private Queue<System.Action> commandQueue = new Queue<System.Action>();
    private object queueLock = new object();

    void Start()
    {
        StartTCPServer();
    }

    void OnDestroy()
    {
        StopTCPServer();
    }

    void Update()
    {
        lock (queueLock)
        {
            while (commandQueue.Count > 0)
            {
                System.Action action = commandQueue.Dequeue();
                action.Invoke();
            }
        }
    }

    void StartTCPServer()
    {
        isServerRunning = true;
        serverThread = new Thread(ListenForClients);
        serverThread.IsBackground = true;
        serverThread.Start();
        
        string localIP = GetLocalIPAddress();
        serverIP = $"http://{localIP}:{port}";
        serverStatus = "Running";
        Debug.Log($"TCP Server started on {serverIP}");
    }

    void StopTCPServer()
    {
        isServerRunning = false;
        serverStatus = "Stopped";
        if (tcpListener != null)
            tcpListener.Stop();
    }

    void ListenForClients()
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        while (isServerRunning)
        {
            TcpClient client = tcpListener.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClientRequest(client));
            clientThread.IsBackground = true;
            clientThread.Start();
        }
    }

    void HandleClientRequest(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string response = ProcessRequest(request);

            byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBuffer, 0, responseBuffer.Length);
            stream.Close();
            client.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error handling client: " + e.Message);
        }
    }

    string ProcessRequest(string request)
    {
        // Parse HTTP request
        string[] lines = request.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        string requestLine = lines[0];
        string[] parts = requestLine.Split(' ');

        if (parts.Length < 2)
            return GetHttpResponse(400, "Bad Request");

        string method = parts[0];
        string path = parts[1];

        if (method != "POST" && method != "GET")  
            return GetHttpResponse(405, "Method Not Allowed");

        // Extract body (JSON or form data)
        string body = "";
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == "")
            {
                if (i + 1 < lines.Length)
                    body = lines[i + 1];
                break;
            }
        }

        return RouteRequest(path, body);
    }

    string RouteRequest(string path, string body)
    {
        if (path.Contains("/tv/on"))
        {
            lock (queueLock)
                commandQueue.Enqueue(() => tv.On());
            return GetHttpResponse(200, "{\"status\":\"TV turned ON\"}");
        }
        else if (path.Contains("/tv/off"))
        {
            lock (queueLock)
                commandQueue.Enqueue(() => tv.Off());
            return GetHttpResponse(200, "{\"status\":\"TV turned OFF\"}");
        }
        else if (path.Contains("/tv/next"))
        {
            lock (queueLock)
                commandQueue.Enqueue(() => tv.Next());
            return GetHttpResponse(200, "{\"status\":\"TV channel changed\"}");
        }
        else if (path.Contains("/tv/previous"))
        {
            lock (queueLock)
                commandQueue.Enqueue(() => tv.Previous());
            return GetHttpResponse(200, "{\"status\":\"TV previous channel\"}");
        }
        else if (path.Contains("/music/on"))
        {
            lock (queueLock)
                commandQueue.Enqueue(() => musicSpeaker.On());
            return GetHttpResponse(200, "{\"status\":\"Music turned ON\"}");
        }
        else if (path.Contains("/music/off"))
        {
            lock (queueLock)
                commandQueue.Enqueue(() => musicSpeaker.Off());
            return GetHttpResponse(200, "{\"status\":\"Music turned OFF\"}");
        }
        else if (path.Contains("/music/next"))
        {
            lock (queueLock)
                commandQueue.Enqueue(() => musicSpeaker.Next());
            return GetHttpResponse(200, "{\"status\":\"Music track changed\"}");
        }
        else if (path.Contains("/tv/volume/"))
        {
            string volumeStr = path.Substring(path.LastIndexOf("/") + 1);
            if (float.TryParse(volumeStr, out float volume))
            {
                volume = Mathf.Clamp01(volume);
                lock (queueLock)
                    commandQueue.Enqueue(() => tv.setVolume(volume));
                return GetHttpResponse(200, $"{{\"status\":\"TV volume set to {volume}\"}}");
            }
            return GetHttpResponse(400, "{\"error\":\"Invalid volume value\"}");
        }
        else if (path.Contains("/music/volume/"))
        {
            string volumeStr = path.Substring(path.LastIndexOf("/") + 1);
            if (float.TryParse(volumeStr, out float volume))
            {
                volume = Mathf.Clamp01(volume);
                lock (queueLock)
                    commandQueue.Enqueue(() => musicSpeaker.audioSource.volume = volume);
                return GetHttpResponse(200, $"{{\"status\":\"Music volume set to {volume}\"}}");
            }
            return GetHttpResponse(400, "{\"error\":\"Invalid volume value\"}");
        }
        else if (path.Contains("/status"))
        {
            string status = $"{{\"tv_on\":{tv.ison},\"music_on\":{musicSpeaker.ison},\"tv_channel\":{tv.cureentchanel},\"music_track\":{musicSpeaker.currentTrack}}}";
            return GetHttpResponse(200, status);
        }
        else
        {
            return GetHttpResponse(404, "Not Found");
        }
    }

    string GetHttpResponse(int statusCode, string body)
    {
        string statusMessage = statusCode == 200 ? "OK" : (statusCode == 400 ? "Bad Request" : (statusCode == 404 ? "Not Found" : "Method Not Allowed"));
        
        StringBuilder response = new StringBuilder();
        response.AppendLine($"HTTP/1.1 {statusCode} {statusMessage}");
        response.AppendLine("Content-Type: application/json");
        response.AppendLine($"Content-Length: {body.Length}");
        response.AppendLine("Connection: close");
        response.AppendLine();
        response.Append(body);

        return response.ToString();
    }

    string GetLocalIPAddress()
    {
        string hostName = System.Net.Dns.GetHostName();
        System.Net.IPHostEntry ipHostInfo = System.Net.Dns.GetHostEntry(hostName);
        
        foreach (System.Net.IPAddress ip in ipHostInfo.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        
        return "127.0.0.1";
    }
}
