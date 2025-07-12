using UnityEngine;

public class NetworkedEntity : MonoBehaviour
{
    // Placeholder for sending position/rotation updates to the network layer
    protected virtual void SyncTransform()
    {
        // TODO: Integrate with Elixir backend
        Debug.Log($"Syncing {name} position {transform.position}");
    }

    protected virtual void LateUpdate()
    {
        SyncTransform();
    }
}
