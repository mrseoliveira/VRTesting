Shader "Custom/MountOliveSkybox"
{
    Properties
    {
        [Header(Sky Gradient)]
        _TopColor       ("Top Color",     Color) = (0.10, 0.35, 0.78, 1)
        _MidColor       ("Mid Color",     Color) = (0.45, 0.70, 0.95, 1)
        _HorizonColor   ("Horizon Color", Color) = (0.82, 0.91, 0.98, 1)
        _TopBlend       ("Top Blend",     Range(0.01, 1)) = 0.45
        _HorizonBlend   ("Horizon Blend", Range(0.01, 1)) = 0.25

        [Header(Clouds)]
        _CloudColor     ("Cloud Color",   Color) = (0.97, 0.98, 1.00, 1)
        _CloudScale     ("Cloud Scale",   Float) = 2.8
        _CloudSpeed     ("Cloud Speed",   Float) = 0.012
        _CloudDensity   ("Cloud Density", Range(0.0, 0.9)) = 0.42
        _CloudSoftness  ("Cloud Softness",Range(0.05, 0.5)) = 0.25
        _CloudBrightness("Cloud Brightness", Range(0.5, 1)) = 0.90

        [Header(Divine Light)]
        _LightDir       ("Light Direction (XYZ)", Vector) = (0.3, 0.6, 0.5, 0)
        _LightColor     ("Light Color",   Color) = (1.0, 0.95, 0.80, 1)
        _LightHalo      ("Light Halo Size", Range(0.01, 0.5)) = 0.12
        _LightIntensity ("Light Intensity", Range(0.0, 3.0)) = 1.4
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 texcoord : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            fixed4  _TopColor, _MidColor, _HorizonColor;
            float   _TopBlend, _HorizonBlend;
            fixed4  _CloudColor;
            float   _CloudScale, _CloudSpeed, _CloudDensity, _CloudSoftness, _CloudBrightness;
            fixed4  _LightColor;
            float4  _LightDir;
            float   _LightHalo, _LightIntensity;

            // --- gradient noise 3D (Perlin-style) sem costuras e sem linhas de grelha ---
            float3 gradHash(float3 p)
            {
                p = float3(dot(p, float3(127.1, 311.7,  74.7)),
                           dot(p, float3(269.5, 183.3, 246.1)),
                           dot(p, float3(113.5, 271.9, 124.6)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453);
            }

            float gradNoise3(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                // interpolação quíntica: C2 contínua — elimina linhas de grelha
                float3 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

                return lerp(
                    lerp(lerp(dot(gradHash(i + float3(0,0,0)), f - float3(0,0,0)),
                              dot(gradHash(i + float3(1,0,0)), f - float3(1,0,0)), u.x),
                         lerp(dot(gradHash(i + float3(0,1,0)), f - float3(0,1,0)),
                              dot(gradHash(i + float3(1,1,0)), f - float3(1,1,0)), u.x), u.y),
                    lerp(lerp(dot(gradHash(i + float3(0,0,1)), f - float3(0,0,1)),
                              dot(gradHash(i + float3(1,0,1)), f - float3(1,0,1)), u.x),
                         lerp(dot(gradHash(i + float3(0,1,1)), f - float3(0,1,1)),
                              dot(gradHash(i + float3(1,1,1)), f - float3(1,1,1)), u.x), u.y),
                    u.z) * 0.5 + 0.5;
            }

            // fBm 3D com gradient noise
            float cloudFbm(float3 p)
            {
                float v = 0.0, a = 0.5;
                for (int n = 0; n < 4; n++)
                {
                    v += a * gradNoise3(p);
                    p  = p * 2.1 + float3(3.7, 1.9, 2.3);
                    a *= 0.48;
                }
                return v;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.texcoord;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float  y   = dir.y;

                // --- gradiente de céu ---
                half4 sky;
                if (y >= 0)
                {
                    float t = smoothstep(0.0, _TopBlend, y);
                    sky = lerp(_MidColor, _TopColor, t);
                }
                else
                {
                    float t = smoothstep(0.0, _HorizonBlend, -y);
                    sky = lerp(_MidColor, _HorizonColor, t);
                }

                // --- nuvens (só hemisfério superior) ---
                if (y > 0.0)
                {
                    // Ruído 3D direto sobre a esfera: sem UV, sem atan2, sem costuras
                    float3 cloudDir = dir * _CloudScale;
                    cloudDir.x += _Time.y * _CloudSpeed; // vento horizontal

                    float c = cloudFbm(cloudDir);
                    float mask = smoothstep(_CloudDensity, _CloudDensity + _CloudSoftness, c);

                    // Fade suave perto do horizonte
                    float horizonFade = smoothstep(0.08, 0.40, y);
                    mask *= horizonFade;

                    half4 cloudTint = lerp(sky * _CloudBrightness, _CloudColor, 0.6);
                    sky = lerp(sky, cloudTint, mask);
                }

                // --- halo de luz divina ---
                float3 lightDir = normalize(_LightDir.xyz);
                float  cosA     = dot(dir, lightDir);
                float  halo     = smoothstep(1.0 - _LightHalo, 1.0, cosA);
                sky += _LightColor * halo * _LightIntensity;

                return half4(sky.rgb, 1);
            }
            ENDCG
        }
    }
}
