using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles spawning and movement of remote players received from the backend.
/// Listens to PhoenixChatClient messages on a zone channel and spawns/destroys
/// remote player GameObjects accordingly.
/// </summary>
public class RemotePlayerManager : MonoBehaviour
{
    [Tooltip("Phoenix WebSocket client for receiving zone events")] public PhoenixChatClient chatClient;
    [Tooltip("Zone identifier to subscribe to")] public string zoneId = "1";
    [Tooltip("Prefab for remote player visuals")] public GameObject remotePlayerPrefab;
    [Tooltip("Parent transform for spawned remote players")] public Transform remotePlayersParent;

    private readonly Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();

    void Start()
    {
        if (chatClient != null)
        {
            chatClient.OnMessage += OnSocketMessage;
            chatClient.JoinChannel($"zone:{zoneId}");
        }
    }

    void OnDestroy()
    {
        if (chatClient != null)
            chatClient.OnMessage -= OnSocketMessage;
    }

    // Expected payload format: { "id": "player1", "position": {"x":0,"y":0,"z":0} }
    // or { "id": "player1", "delta": {"x":0,"y":0,"z":1} }
    void OnSocketMessage(PhoenixMessage msg)
    {
        if (!msg.topic.StartsWith("zone:"))
            return;

        if (msg.@event == "player_joined")
        {
            HandlePlayerJoined(msg.payload);
        }
        else if (msg.@event == "player_moved")
        {
            HandlePlayerMoved(msg.payload);
        }
        else if (msg.@event == "player_left")
        {
            HandlePlayerLeft(msg.payload);
        }
    }

    void HandlePlayerJoined(object payload)
    {
        if (remotePlayerPrefab == null)
            return;

        // use JsonUtility to parse generic payload
        string json = UnityEngine.JsonUtility.ToJson(payload);
        var data = JsonUtility.FromJson<PlayerJoinData>(json);
        if (data == null || string.IsNullOrEmpty(data.id))
            return;

        if (remotePlayers.ContainsKey(data.id))
            return;

        Transform parent = remotePlayersParent != null ? remotePlayersParent : transform;
        GameObject obj = Instantiate(remotePlayerPrefab, parent);
        obj.name = $"Remote_{data.id}";
        obj.transform.position = data.position.ToVector3();
        remotePlayers[data.id] = obj;
    }

    void HandlePlayerMoved(object payload)
    {
        string json = UnityEngine.JsonUtility.ToJson(payload);
        var data = JsonUtility.FromJson<PlayerMoveData>(json);
        if (data == null || string.IsNullOrEmpty(data.id))
            return;
        if (remotePlayers.TryGetValue(data.id, out GameObject obj))
        {
            // simple position update using delta movement
            obj.transform.position += data.delta.ToVector3();
        }
    }

    void HandlePlayerLeft(object payload)
    {
        string json = UnityEngine.JsonUtility.ToJson(payload);
        var data = JsonUtility.FromJson<PlayerLeftData>(json);
        if (data == null || string.IsNullOrEmpty(data.id))
            return;
        if (remotePlayers.TryGetValue(data.id, out GameObject obj))
        {
            Destroy(obj);
            remotePlayers.Remove(data.id);
        }
    }

    [System.Serializable]
    class PlayerJoinData
    {
        public string id;
        public SerializableVector3 position;
    }

    [System.Serializable]
    class PlayerMoveData
    {
        public string id;
        public SerializableVector3 delta;
    }

    [System.Serializable]
    class PlayerLeftData
    {
        public string id;
    }

    [System.Serializable]
    struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }
}
