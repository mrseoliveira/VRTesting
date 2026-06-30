using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateSandTexture
{
    [MenuItem("Tools/Monte das Oliveiras/Criar Textura Areia Dourada")]
    static void Create()
    {
        const int size = 512;
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true);

        // Paleta de areia dourada do Médio Oriente
        var sandLight  = new Color(0.91f, 0.82f, 0.58f);
        var sandMid    = new Color(0.80f, 0.70f, 0.44f);
        var sandDark   = new Color(0.68f, 0.57f, 0.32f);

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)size;
                float ny = y / (float)size;

                // Várias camadas de ruído para variação orgânica
                float n  = Fbm(nx * 6f,  ny * 6f,  4);
                float n2 = Fbm(nx * 14f, ny * 14f, 2) * 0.25f;
                float n3 = Fbm(nx * 30f, ny * 30f, 1) * 0.08f;
                float v  = Mathf.Clamp01(n + n2 + n3);

                Color c = v < 0.45f
                    ? Color.Lerp(sandDark,  sandMid,   v / 0.45f)
                    : Color.Lerp(sandMid,   sandLight, (v - 0.45f) / 0.55f);

                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        const string path = "Assets/Textures/sand_dourada.png";
        Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath + "/" + path.Replace("Assets/", "")));
        File.WriteAllBytes(Application.dataPath + path.Replace("Assets", ""), tex.EncodeToPNG());
        AssetDatabase.Refresh();

        // Atribuir ao terrain layer
        var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/SandLayer.terrainlayer");
        if (layer != null)
        {
            var newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (newTex != null)
            {
                layer.diffuseTexture = newTex;
                layer.tileSize = new Vector2(4f, 4f);
                EditorUtility.SetDirty(layer);
                AssetDatabase.SaveAssets();
                Debug.Log("[Monte das Oliveiras] Textura de areia dourada aplicada ao SandLayer.");
            }
        }
        else
        {
            Debug.LogWarning("[Monte das Oliveiras] SandLayer.terrainlayer não encontrado. Atribui a textura manualmente.");
        }
    }

    static float Fbm(float x, float y, int octaves)
    {
        float v = 0f, a = 0.5f, freq = 1f;
        for (int i = 0; i < octaves; i++)
        {
            v    += a * Mathf.PerlinNoise(x * freq + i * 31.7f, y * freq + i * 17.3f);
            freq *= 2f;
            a    *= 0.5f;
        }
        return v;
    }
}
