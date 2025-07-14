using UnityEngine;

public class EnsureTerrain : MonoBehaviour
{
    void Start()
    {
        // Create Terrain if it doesn't exist
        if (Object.FindAnyObjectByType<Terrain>() == null)
        {
            GameObject terrainGO = new GameObject("Terrain");
            terrainGO.transform.position = Vector3.zero;

            Terrain terrain = terrainGO.AddComponent<Terrain>();
            TerrainCollider col = terrainGO.AddComponent<TerrainCollider>();

            TerrainData data = new TerrainData();
            data.heightmapResolution = 33;
            data.size = new Vector3(500f, 600f, 500f);
            data.SetHeights(0, 0, new float[data.heightmapResolution, data.heightmapResolution]);

            terrain.terrainData = data;
            col.terrainData = data;

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.green;
            terrain.materialTemplate = mat;
        }

        // Fallback plane for simple collision
        if (GameObject.Find("FallbackPlane") == null)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "FallbackPlane";
            plane.transform.position = Vector3.zero;
            plane.transform.localScale = Vector3.one;
        }
    }
}
