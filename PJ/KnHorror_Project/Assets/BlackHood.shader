Shader "Custom/BlackFogGlitchDissolveRim"
{
    Properties
    {
        _MainTex ("Texture (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise Texture (Dissolve/Glitch)", 2D) = "white" {}
        _Color ("Main Color", Color) = (0.1, 0.1, 0.1, 1) // โทนดำดาร์กๆ ลึกลับ
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _BurnSize ("Edge Glow Size", Range(0.0, 0.2)) = 0.05
        _BurnColor ("Edge Color (Dissolve Glow)", Color) = (0.2, 0, 0.4, 1) // ขอบเรืองแสงตอนสลาย
        
        // --- ค่าสำหรับ Rim Light ---
        _RimColor ("Rim Light Color", Color) = (0, 0.8, 1, 1) // สีขอบเรืองแสง (ค่าเริ่มต้นฟ้าวิญญาณ)
        _RimPower ("Rim Power (Sharpness)", Range(0.5, 8.0)) = 3.0 // ความฟุ้ง/คมของขอบ
        _RimIntensity ("Rim Intensity", Range(0.0, 5.0)) = 1.5 // ความสว่างออร่า

        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.3
        _Speed ("Speed", Range(1, 20)) = 10
        _FogSpeed ("Fog Flow Speed", Vector) = (0.1, 0.1, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "BlackFogGlitchRimPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 screenPos    : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float3 viewDirWS    : TEXCOORD3;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            Texture2D _NoiseTex;
            SamplerState sampler_NoiseTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float4 _Color;
                float _DissolveAmount;
                float _BurnSize;
                float4 _BurnColor;
                float4 _RimColor;
                float _RimPower;
                float _RimIntensity;
                float _GlitchIntensity;
                float _Speed;
                float2 _FogSpeed;
            CBUFFER_END

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float time = _Time.y * _Speed;
                float4 clipPos = TransformObjectToHClip(input.positionOS.xyz);
                
                // เอฟเฟกต์สั่นกระตุกแบบ Glitch
                float2 shakeOffset = (random(floor(clipPos.xy * 50.0 + time)) - 0.5) * _GlitchIntensity * 0.1;
                clipPos.xy += shakeOffset;

                output.positionCS = clipPos;
                output.screenPos = ComputeScreenPos(clipPos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // คำนวณ Normal และ View Direction สำหรับทำ Rim Light
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _Speed;
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // 1. Dissolve & Flow หมอก
                float2 animatedUV = input.uv + (_Time.y * _FogSpeed);
                float noiseVal = _NoiseTex.Sample(sampler_NoiseTex, animatedUV).r;

                float dissolveThreshold = _DissolveAmount * 1.1;
                float diff = noiseVal - dissolveThreshold;

                if (diff < 0.0)
                {
                    discard;
                }

                // 2. Glitch หน้าจอ
                float noiseBlock = random(floor(screenUV * 30.0 + time));
                float2 uv = screenUV;

                if (noiseBlock > (1.0 - _GlitchIntensity * 0.5))
                {
                    uv.x += (random(float2(time, screenUV.y)) - 0.5) * _GlitchIntensity * 0.3;
                }

                float splitAmount = _GlitchIntensity * 0.04 * noiseBlock;

                half4 colR = _MainTex.Sample(sampler_MainTex, uv + float2(splitAmount, 0));
                half4 colG = _MainTex.Sample(sampler_MainTex, uv);
                half4 colB = _MainTex.Sample(sampler_MainTex, uv - float2(splitAmount, 0));

                half4 finalCol;
                finalCol.r = colR.r;
                finalCol.g = colG.g;
                finalCol.b = colB.b;
                finalCol.a = (colR.a + colG.a + colB.a) / 3.0;

                finalCol *= _Color;

                // 3. คำนวณ Rim Light (แสงเรืองขอบตามมุมมองกล้อง)
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                float rim = 1.0 - saturate(dot(viewDir, normal));
                rim = pow(rim, _RimPower);
                
                half4 rimColor = _RimColor * rim * _RimIntensity;
                finalCol.rgb += rimColor.rgb; // เอาแสง Rim ไปบวกเพิ่มความสว่างตรงขอบ

                // 4. ขอบเรืองแสงตอน Dissolve
                if (diff < _BurnSize && _DissolveAmount > 0.0)
                {
                    float t = diff / _BurnSize;
                    finalCol = lerp(_BurnColor, finalCol, t);
                }

                // 5. สแกนไลน์ & กระพริบหาย
                float scanline = sin(screenUV.y * 500.0 + time * 15.0) * 0.06 * _GlitchIntensity;
                finalCol.rgb -= scanline;

                if (random(float2(time, 4.0)) > 0.93)
                {
                    finalCol.a *= 0.05;
                }

                return finalCol;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}