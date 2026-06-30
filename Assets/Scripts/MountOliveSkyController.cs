using UnityEngine;
using System.Collections;

public class MountOliveSkyController : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material skyboxMaterial;

    [Header("Divine Response")]
    [SerializeField] private float transitionDuration = 3.0f;

    // IDs cached para performance (evita string lookup a cada frame)
    private static readonly int TopColorID      = Shader.PropertyToID("_TopColor");
    private static readonly int MidColorID      = Shader.PropertyToID("_MidColor");
    private static readonly int HorizonColorID  = Shader.PropertyToID("_HorizonColor");
    private static readonly int LightIntensityID= Shader.PropertyToID("_LightIntensity");
    private static readonly int CloudDensityID  = Shader.PropertyToID("_CloudDensity");

    // Estado base (Monte das Oliveiras, sereno)
    private readonly Color baseTop     = new Color(0.10f, 0.35f, 0.78f);
    private readonly Color baseMid     = new Color(0.45f, 0.70f, 0.95f);
    private readonly Color baseHorizon = new Color(0.82f, 0.91f, 0.98f);
    private const float baseLightIntensity = 1.4f;
    private const float baseCloudDensity  = 0.42f;

    // Estado "presença divina" — luz mais quente, céu mais profundo
    private readonly Color divineTop     = new Color(0.05f, 0.15f, 0.55f);
    private readonly Color divineMid     = new Color(0.25f, 0.50f, 0.85f);
    private readonly Color divineHorizon = new Color(0.90f, 0.85f, 0.65f);
    private const float divineLightIntensity = 2.8f;
    private const float divineCloudDensity  = 0.28f;

    private Coroutine activeTransition;

    void Start()
    {
        if (skyboxMaterial == null)
            skyboxMaterial = RenderSettings.skybox;

        ApplyImmediate(baseTop, baseMid, baseHorizon, baseLightIntensity, baseCloudDensity);
    }

    // Chamado externamente (pelo sistema de voz, por exemplo)
    public void OnDivinePresence()
    {
        StartTransition(divineTop, divineMid, divineHorizon, divineLightIntensity, divineCloudDensity);
    }

    public void OnReturnToCalm()
    {
        StartTransition(baseTop, baseMid, baseHorizon, baseLightIntensity, baseCloudDensity);
    }

    private void StartTransition(Color top, Color mid, Color horizon, float lightInt, float cloudDens)
    {
        if (activeTransition != null)
            StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionSky(top, mid, horizon, lightInt, cloudDens));
    }

    private IEnumerator TransitionSky(Color toTop, Color toMid, Color toHorizon, float toLightInt, float toCloudDens)
    {
        Color fromTop     = skyboxMaterial.GetColor(TopColorID);
        Color fromMid     = skyboxMaterial.GetColor(MidColorID);
        Color fromHorizon = skyboxMaterial.GetColor(HorizonColorID);
        float fromLight   = skyboxMaterial.GetFloat(LightIntensityID);
        float fromCloud   = skyboxMaterial.GetFloat(CloudDensityID);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            skyboxMaterial.SetColor(TopColorID,      Color.Lerp(fromTop,     toTop,     t));
            skyboxMaterial.SetColor(MidColorID,      Color.Lerp(fromMid,     toMid,     t));
            skyboxMaterial.SetColor(HorizonColorID,  Color.Lerp(fromHorizon, toHorizon, t));
            skyboxMaterial.SetFloat(LightIntensityID, Mathf.Lerp(fromLight,  toLightInt, t));
            skyboxMaterial.SetFloat(CloudDensityID,   Mathf.Lerp(fromCloud,  toCloudDens, t));

            yield return null;
        }

        ApplyImmediate(toTop, toMid, toHorizon, toLightInt, toCloudDens);
    }

    private void ApplyImmediate(Color top, Color mid, Color horizon, float lightInt, float cloudDens)
    {
        skyboxMaterial.SetColor(TopColorID,      top);
        skyboxMaterial.SetColor(MidColorID,      mid);
        skyboxMaterial.SetColor(HorizonColorID,  horizon);
        skyboxMaterial.SetFloat(LightIntensityID, lightInt);
        skyboxMaterial.SetFloat(CloudDensityID,   cloudDens);
    }
}
