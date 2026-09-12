Shader "Custom/UIUnderwater"
{
    Properties
    {
        [HideInInspector] _MainTex ("UI Texture", 2D) = "white" {}
        [HideInInspector] _Color ("UI Tint", Color) = (1,1,1,1)

        _WaterColor ("Water Tint", Color) = (0.35, 0.75, 0.85, 0.35)

        _Distortion ("Water Distortion", Range(0, 0.2)) = 0.045
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 0.8
        _WaveFrequency ("Wave Frequency", Range(1, 30)) = 8

        _FlowX ("Water Flow X", Range(-1, 1)) = 0.15
        _FlowY ("Water Flow Y", Range(-1, 1)) = 0.08

        _Wobble ("Wobble Strength", Range(0, 0.15)) = 0.025
        _WobbleSpeed ("Wobble Speed", Range(0, 5)) = 1.2

        _CausticColor ("Caustic Color", Color) = (0.55, 1.0, 0.95, 0.25)
        _CausticStrength ("Caustic Strength", Range(0, 2)) = 0.45
        _CausticScale ("Caustic Scale", Range(1, 15)) = 5

        _Softness ("Edge Softness", Range(0, 0.2)) = 0.04
        _EdgeGlow ("Edge Glow", Range(0, 2)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanBeSliced"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIUnderwater"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;

                float4 _WaterColor;

                float _Distortion;
                float _DistortionSpeed;
                float _WaveFrequency;

                float _FlowX;
                float _FlowY;

                float _Wobble;
                float _WobbleSpeed;

                float4 _CausticColor;
                float _CausticStrength;
                float _CausticScale;

                float _Softness;
                float _EdgeGlow;

            CBUFFER_END


            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));

                p += dot(
                    p,
                    p + 45.32
                );

                return frac(
                    p.x * p.y
                );
            }


            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }


            float2 WaterNoise(float2 uv)
            {
                float time =
                    _Time.y *
                    _DistortionSpeed;

                float2 p =
                    uv *
                    _WaveFrequency;

                p +=
                    float2(
                        time * _FlowX,
                        time * _FlowY
                    );


                float n1 =
                    noise(
                        p
                    );

                float n2 =
                    noise(
                        p * 1.7
                        + time * 0.35
                    );


                float waveX =
                    sin(
                        p.y
                        + n1 * 4.0
                        + time
                    );

                float waveY =
                    cos(
                        p.x
                        + n2 * 4.0
                        - time * 0.8
                    );


                float2 distortion =
                    float2(
                        waveX,
                        waveY
                    );


                distortion +=
                    (float2(
                        n1,
                        n2
                    ) - 0.5) *
                    0.8;


                return distortion *
                       _Distortion;
            }


            float Caustic(float2 uv)
            {
                float time =
                    _Time.y *
                    _DistortionSpeed;


                float2 p =
                    uv *
                    _CausticScale;


                p +=
                    float2(
                        time * 0.15,
                        -time * 0.08
                    );


                float n1 =
                    noise(
                        p
                        + sin(
                            p.yx * 1.5
                            + time
                        )
                    );


                float n2 =
                    noise(
                        p * 2.0
                        - time * 0.2
                    );


                float c =
                    n1 *
                    n2;


                c =
                    smoothstep(
                        0.35,
                        0.75,
                        c
                    );


                return c;
            }


            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );

                output.positionCS =
                    vertexInput.positionCS;

                output.uv =
                    input.uv;

                output.color =
                    input.color *
                    _Color;

                return output;
            }


            float4 frag(Varyings input) : SV_Target
            {
                float2 uv =
                    input.uv;


                // =================================
                // UNDERWATER WOBBLE
                // =================================

                float wobbleTime =
                    _Time.y *
                    _WobbleSpeed;


                float wobbleX =
                    sin(
                        uv.y * 8.0
                        + wobbleTime
                    ) *
                    _Wobble;


                float wobbleY =
                    cos(
                        uv.x * 7.0
                        + wobbleTime * 0.8
                    ) *
                    _Wobble;


                uv +=
                    float2(
                        wobbleX,
                        wobbleY
                    );


                // =================================
                // WATER DISTORTION
                // =================================

                float2 distortion =
                    WaterNoise(uv);


                float2 distortedUV =
                    uv +
                    distortion;


                // =================================
                // FLOW
                // =================================

                distortedUV +=
                    float2(
                        _FlowX,
                        _FlowY
                    ) *
                    _Time.y *
                    0.01;


                // =================================
                // SAMPLE UI
                // =================================

                float4 tex =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        distortedUV
                    );


                // =================================
                // WATER TINT
                // =================================

                float distortionAmount =
                    length(
                        distortion
                    );


                float waterFactor =
                    saturate(
                        distortionAmount *
                        8.0
                    );


                float3 finalColor =
                    tex.rgb;


                finalColor =
                    lerp(
                        finalColor,
                        finalColor *
                        _WaterColor.rgb,
                        _WaterColor.a
                    );


                // =================================
                // CAUSTIC LIGHT
                // =================================

                float caustic =
                    Caustic(
                        uv
                    );


                finalColor +=
                    _CausticColor.rgb *
                    caustic *
                    _CausticStrength;


                // =================================
                // LIGHT WAVES
                // =================================

                float lightWave =
                    sin(
                        uv.x * 12.0
                        + uv.y * 8.0
                        + _Time.y * 0.7
                    );


                lightWave =
                    lightWave * 0.5
                    + 0.5;


                lightWave =
                    smoothstep(
                        0.72,
                        1.0,
                        lightWave
                    );


                finalColor +=
                    _CausticColor.rgb *
                    lightWave *
                    0.06;


                // =================================
                // EDGE LIGHT
                // =================================

                float edge =
                    min(
                        min(
                            uv.x,
                            1.0 - uv.x
                        ),
                        min(
                            uv.y,
                            1.0 - uv.y
                        )
                    );


                float edgeGlow =
                    smoothstep(
                        _Softness,
                        0.0,
                        edge
                    );


                finalColor +=
                    _WaterColor.rgb *
                    edgeGlow *
                    _EdgeGlow;


                // =================================
                // ALPHA
                // =================================

                float alpha =
                    tex.a *
                    input.color.a;


                return float4(
                    finalColor,
                    alpha
                );
            }

            ENDHLSL
        }
    }
}