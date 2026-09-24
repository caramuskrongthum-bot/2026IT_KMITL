Shader "Custom/DualSmokeBackground"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        
        // ควันอันที่ 1 (จางน้อยกว่า) - ปรับความเร็วเฉพาะแกน X ได้เลยแม่
        [Header(Smoke Layer 1)]
        _MainTex ("Smoke Texture 1", 2D) = "white" {}
        _ScrollSpeedX1 ("Scroll Speed X 1", Float) = 0.05
        _Alpha1 ("Opacity (จางน้อยกว่า)", Range(0, 1)) = 0.8

        // ควันอันที่ 2 (จางมาก) - ปรับความเร็วเฉพาะแกน X ได้เลยแม่
        [Header(Smoke Layer 2)]
        _SmokeTex2 ("Smoke Texture 2", 2D) = "white" {}
        _ScrollSpeedX2 ("Scroll Speed X 2", Float) = -0.03
        _Alpha2 ("Opacity (จางมาก)", Range(0, 1)) = 0.4

        // ระยะบิดเบี้ยว Normal (ให้มันเบี้ยวแนวนอนพริ้วๆ)
        [Header(Distortion)]
        _NormalMap ("Normal Map (Optional)", 2D) = "bump" {}
        _DistortionStrength ("Distortion Strength", Range(0, 0.5)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        LOD 100
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Unlit"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_SmokeTex2);
            SAMPLER(sampler_SmokeTex2);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SmokeTex2_ST;
                float4 _NormalMap_ST;
                float4 _BaseColor;
                float _ScrollSpeedX1;
                float _Alpha1;
                float _ScrollSpeedX2;
                float _Alpha2;
                float _DistortionStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. ทำ Normal Map ให้ควันมันเบี้ยวๆ แนวนอนพริ้วๆ (ล็อก Y ไว้ไม่ให้ขยับขึ้นลง)
                float2 normalUV = input.uv * _NormalMap_ST.xy + _NormalMap_ST.zw;
                normalUV.x += _Time.y * 0.01; 
                float4 normalTex = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, normalUV);
                float2 distortion = (normalTex.rg * 2.0 - 1.0) * _DistortionStrength;

                // 2. ควันชั้นที่ 1 (เลื่อนเฉพาะแกน X + ใส่ความเบี้ยว)
                float2 uv1 = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                uv1.x += _Time.y * _ScrollSpeedX1 + distortion.x;
                float smoke1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1).r * _Alpha1;

                // 3. ควันชั้นที่ 2 (เลื่อนเฉพาะแกน X คนละทิศ + ใส่ความเบี้ยว)
                float2 uv2 = input.uv * _SmokeTex2_ST.xy + _SmokeTex2_ST.zw;
                uv2.x += _Time.y * _ScrollSpeedX2 + distortion.x;
                float smoke2 = SAMPLE_TEXTURE2D(_SmokeTex2, sampler_SmokeTex2, uv2).r * _Alpha2;

                // 4. รวมควันทั้งสองชั้น
                float finalSmoke = saturate(smoke1 + smoke2);

                float4 finalColor = _BaseColor;
                finalColor.a *= finalSmoke;

                return finalColor;
            }
            ENDHLSL
        }
    }
}