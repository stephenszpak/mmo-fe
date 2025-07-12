using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingName : MonoBehaviour
{
    public string entityName = "NPC";
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public float scale = 0.05f;

    private TextMeshProUGUI textMesh;

    void Start()
    {
        GameObject canvasGO = new GameObject("NameCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = offset;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one * scale;

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        canvasGO.AddComponent<FaceCamera>();

        GameObject textGO = new GameObject("NameLabel");
        textGO.transform.SetParent(canvasGO.transform, false);
        textMesh = textGO.AddComponent<TextMeshProUGUI>();
        textMesh.text = entityName;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 24;

        var rect = textMesh.rectTransform;
        rect.sizeDelta = new Vector2(200, 50);
    }

    public void SetName(string newName)
    {
        entityName = newName;
        if (textMesh != null)
            textMesh.text = entityName;
    }
}
