using UnityEngine;

/// Attach this to any GameObject in the scene, enter Play Mode, and check the Console.
/// Remove after use.
public class FindSpawnHeight : MonoBehaviour
{
    [Tooltip("A posição X,Z onde o XR Origin começa")]
    public Vector3 spawnPosition = new Vector3(2106f, 0f, 2528f);

    void Start()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("Nenhum Terrain activo encontrado na cena.");
            return;
        }

        float height = terrain.SampleHeight(spawnPosition);
        float worldY = terrain.transform.position.y + height;

        Debug.Log($"[FindSpawnHeight] Altura do terreno em ({spawnPosition.x}, {spawnPosition.z}) = {worldY:F3} metros");
        Debug.Log($"[FindSpawnHeight] Define o Y do XR Origin para: {worldY:F3}");
    }
}
