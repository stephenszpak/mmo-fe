using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        if (chatClient == null)
        {
            chatClient = Object.FindFirstObjectByType<PhoenixChatClient>();
            if (chatClient == null)
            {
                Debug.LogError("RemotePlayerManager: No PhoenixChatClient found");
                return;
            }
            else
            {
                Debug.Log("RemotePlayerManager: found PhoenixChatClient automatically");
            }
        }

        chatClient.OnMessage += OnSocketMessage;
        chatClient.JoinChannel($"zone:{zoneId}");
        Debug.Log($"RemotePlayerManager subscribed to zone:{zoneId}");
    }

    void OnDestroy()
    {
        if (chatClient != null)
        {
            chatClient.OnMessage -= OnSocketMessage;
            Debug.Log("RemotePlayerManager unsubscribed from PhoenixChatClient");
        }
    }

    // Expected payload format: { "id": "player1", "position": {"x":0,"y":0,"z":0} }
    // or { "id": "player1", "delta": {"x":0,"y":0,"z":1} }
    void OnSocketMessage(PhoenixMessage msg)
    {
        if (msg == null)
            return;

        Debug.Log($"📩 Received event {msg.@event} from topic {msg.topic}");

        if (!msg.topic.StartsWith("zone:"))
            return;

        if (msg.@event == "zone_state")
        {
            HandleZoneState(msg.payload);
        }
        else if (msg.@event == "player_joined")
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
        {
            Debug.LogWarning("RemotePlayerManager: remotePlayerPrefab not set");
            return;
        }

        // deserialize payload into strongly typed data
        string json = payload?.ToString();
        var data = JsonConvert.DeserializeObject<PlayerJoinData>(json);
        if (data == null || string.IsNullOrEmpty(data.id))
            return;

        if (remotePlayers.ContainsKey(data.id))
            return;

        SpawnRemotePlayer(data.id, data.position.ToVector3());
    }

    void HandlePlayerMoved(object payload)
    {
        string json = payload?.ToString();
        var data = JsonConvert.DeserializeObject<PlayerMoveData>(json);
        if (data == null || string.IsNullOrEmpty(data.id))
            return;
        if (remotePlayers.TryGetValue(data.id, out GameObject obj))
        {
            // simple position update using delta movement
            obj.transform.position += data.delta.ToVector3();
            Debug.Log($"Moved remote player '{data.id}' to {obj.transform.position}");
        }
    }

    void HandlePlayerLeft(object payload)
    {
        string json = payload?.ToString();
        var data = JsonConvert.DeserializeObject<PlayerLeftData>(json);
        if (data == null || string.IsNullOrEmpty(data.id))
            return;
        if (remotePlayers.TryGetValue(data.id, out GameObject obj))
        {
            Destroy(obj);
            remotePlayers.Remove(data.id);
            Debug.Log($"Removed remote player '{data.id}'");
        }
    }

    void HandleZoneState(object payload)
    {
        string json = payload?.ToString();
        var data = JsonConvert.DeserializeObject<ZoneStateData>(json);
        if (data == null || data.players == null)
        {
            Debug.LogWarning("RemotePlayerManager: invalid zone_state payload");
            return;
        }

        foreach (var p in data.players)
        {
            if (p == null || string.IsNullOrEmpty(p.id))
                continue;

            Vector3 pos = p.position.ToVector3();
            if (remotePlayers.ContainsKey(p.id))
            {
                remotePlayers[p.id].transform.position = pos;
                Debug.Log($"Updated remote player '{p.id}' from zone_state");
            }
            else
            {
                SpawnRemotePlayer(p.id, pos);
            }
        }
    }

    void SpawnRemotePlayer(string id, Vector3 position)
    {
        if (remotePlayerPrefab == null)
        {
            Debug.LogWarning("RemotePlayerManager: remotePlayerPrefab not set");
            return;
        }

        Transform parent = remotePlayersParent != null ? remotePlayersParent : transform;
        GameObject obj = Instantiate(remotePlayerPrefab, position, Quaternion.identity, parent);
        obj.name = $"Remote_{id}";
        remotePlayers[id] = obj;
        Debug.Log($"Spawned remote player '{id}' at {position}");
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
    class ZoneStateData
    {
        public PlayerJoinData[] players;
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
