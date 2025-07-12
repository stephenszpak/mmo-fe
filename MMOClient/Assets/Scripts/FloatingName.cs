using UnityEngine;

public class FloatingName : MonoBehaviour
{
    public string entityName = "NPC";
    public Vector3 offset = new Vector3(0, 2f, 0);
    private TextMesh textMesh;

    void Start()
    {
        GameObject go = new GameObject("NameTag");
        go.transform.SetParent(transform);
        go.transform.localPosition = offset;
        textMesh = go.AddComponent<TextMesh>();
        textMesh.text = entityName;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.2f;
        textMesh.color = Color.white;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
            textMesh.transform.rotation = Camera.main.transform.rotation;
    }
}
