using UnityEngine;
using WebSocketSharp;
using System;

[Serializable]
public class ChatPayload
{
    public string from;
    public string to;
    public string text;
}

[Serializable]
public class PhoenixMessage
{
    public string topic;
    public string @event;
    public ChatPayload payload;
    public string @ref;
    public string join_ref;
}

[Serializable]
public class EmptyPayload { }

[Serializable]
public class JoinMessage
{
    public string topic;
    public string @event;
    public EmptyPayload payload = new EmptyPayload();
    public string @ref;
}

public class PhoenixChatClient : MonoBehaviour
{
    public string socketUrl = "ws://localhost:4001/socket/websocket";
    public string playerName = "player1";
    public string globalTopic = "chat:global";

    private WebSocket socket;
    private int refCounter = 1;

    public event Action<PhoenixMessage> OnChatMessage;

    void Start()
    {
        Connect();
    }

    public void Connect()
    {
        if (socket != null)
            return;
        socket = new WebSocket(socketUrl);
        socket.OnOpen += (s, e) =>
        {
            Debug.Log("Connected to Phoenix socket");
            JoinChannel(globalTopic);
        };
        socket.OnMessage += (s, e) => HandleMessage(e);
        socket.OnError += (s, e) => Debug.LogError("WebSocket error: " + e.Message);
        socket.ConnectAsync();
    }

    void HandleMessage(MessageEventArgs e)
    {
        if (!e.IsText)
            return;
        PhoenixMessage msg = JsonUtility.FromJson<PhoenixMessage>(e.Data);
        if (msg.@event == "message")
        {
            OnChatMessage?.Invoke(msg);
        }
        else if (msg.@event == "phx_reply")
        {
            Debug.Log("Join reply for " + msg.topic);
        }
    }

    void OnDestroy()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
    }

    void JoinChannel(string topic)
    {
        var join = new JoinMessage
        {
            topic = topic,
            @event = "phx_join",
            @ref = (refCounter++).ToString()
        };
        string json = JsonUtility.ToJson(join);
        socket.Send(json);
    }

    public void SendChat(string toTopic, string text)
    {
        if (socket == null || socket.ReadyState != WebSocketState.Open)
            return;

        PhoenixMessage msg = new PhoenixMessage
        {
            topic = toTopic,
            @event = "message",
            payload = new ChatPayload { from = playerName, to = toTopic, text = text },
            @ref = (refCounter++).ToString()
        };
        string json = JsonUtility.ToJson(msg);
        socket.Send(json);
    }
}
