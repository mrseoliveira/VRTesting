using UnityEngine;
using UnityEditor;

public static class FixTreeForQuest
{
    [MenuItem("Tools/Monte das Oliveiras/Corrigir Árvore para Quest")]
    static void Fix()
    {
        // --- 1. Encontrar a árvore na cena ---
        var root = GameObject.Find("Chestnut 5");
        if (root == null)
        {
            Debug.LogError("Chestnut 5 não encontrado na cena. Verifica o nome na Hierarchy.");
            return;
        }

        // --- 2. Desativar sombras em todos os MeshRenderers ---
        // Sombras com Alpha Cutout cintilam muito em Quest
        int rendererCount = 0;
        foreach (var mr in root.GetComponentsInChildren<MeshRenderer>())
        {
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            EditorUtility.SetDirty(mr);
            rendererCount++;
        }

        // --- 3. Suavizar transições de LOD ---
        var lodGroup = root.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            LOD[] lods = lodGroup.GetLODs();

            // Quest tem ecrã pequeno — usar distâncias mais generosas
            // para evitar popping visível
            if (lods.Length >= 4)
            {
                lods[0].screenRelativeTransitionHeight = 0.15f;  // LOD0 até mais longe
                lods[1].screenRelativeTransitionHeight = 0.07f;
                lods[2].screenRelativeTransitionHeight = 0.03f;
                lods[3].screenRelativeTransitionHeight = 0.01f;
            }

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
            EditorUtility.SetDirty(lodGroup);

            Debug.Log($"[FixTree] LOD suavizado. {rendererCount} renderers sem sombras.");
        }

        // --- 4. Reportar altura do terreno em várias posições ---
        var terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            float h0   = terrain.SampleHeight(new Vector3(0,   0, 0))   + terrain.transform.position.y;
            float h200 = terrain.SampleHeight(new Vector3(200, 0, 200)) + terrain.transform.position.y;
            float h500 = terrain.SampleHeight(new Vector3(500, 0, 500)) + terrain.transform.position.y;

            Debug.Log($"[FixTree] Altura terreno em (0,0)     = {h0:F1}");
            Debug.Log($"[FixTree] Altura terreno em (200,200) = {h200:F1}");
            Debug.Log($"[FixTree] Altura terreno em (500,500) = {h500:F1}");
            Debug.Log("[FixTree] Mover XR Origin + árvore para perto de (0,Y,0) elimina o tremido.");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[FixTree] Concluído. Recomenda-se fazer Build novamente.");
    }
}
