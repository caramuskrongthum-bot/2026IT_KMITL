Shader "Custom/URP_StylizedHair_FixDark"
{
    Properties
    {
        [Header(Base Textures)]
        _MainTex ("Base Hair Texture", 2D) = "white" {}
        _TexBrightness ("Main Texture Brightness", Range(0.0, 5.0)) = 1.0
        _BaseColor ("Hair Tint Color", Color) = (0.8, 0.3, 0.5, 1)
        _ShadowColor ("Shadow Color", Color) = (0.5, 0.2, 0.4, 1) // ล็อกไว้ไม่ให้มืดไป

        [Header(Wind Map)]
        _WindMap ("Wind Weight Map (Black = Move, White = Still)", 2D) = "white" {}

        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Range(0, 10)) = 3.0
        _WindFrequency ("Wind Wave Frequency", Range(0, 10)) = 2.0
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.15
        _WindDirection ("Wind Direction (XYZ)", Vector) = (1, 0.2, 0, 0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uvMain     : TEXCOORD1;
                float2 uvWind     : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_WindMap);
            SAMPLER(sampler_WindMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half _TexBrightness;
                float4 _MainTex_ST;
                float4 _WindMap_ST;

                half _WindSpeed;
                half _WindFrequency;
                half _WindStrength;
                half4 _WindDirection;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.uvMain = TRANSFORM_TEX(input.uv, _MainTex);
                output.uvWind = TRANSFORM_TEX(input.uv, _WindMap);

                // Sample Wind Map
                float mapColor = SAMPLE_TEXTURE2D_LOD(_WindMap, sampler_WindMap, output.uvWind, 0).r;
                float windWeight = 1.0 - mapColor;

                // Wind Animation
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float wave = sin(_Time.y * _WindSpeed + (worldPos.x + worldPos.y + worldPos.z) * _WindFrequency);
                float3 windOffset = normalize(_WindDirection.xyz) * wave * _WindStrength * windWeight;

                worldPos += windOffset;

                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. แก้ไข Flat Normal สลับลำดับ Cross Product ชี้ออกด้านนอกตรงๆ
                float3 dpdx = ddx(input.positionWS);
                float3 dpdy = ddy(input.positionWS);
                float3 flatNormal = normalize(cross(dpdx, dpdy)); // สลับเป็น dx cross dy

                // 2. Sample Base Texture + ปรับความสว่าง
                half4 albedoTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvMain);
                half3 brightTex = albedoTex.rgb * _TexBrightness;
                half3 baseAlbedo = brightTex * _BaseColor.rgb;

                // 3. Toon Lighting + แสง Ambient ในฉาก
                Light mainLight = GetMainLight();
                half NdotL = dot(flatNormal, mainLight.direction);
                
                // คำนวณความสว่างแบบผ่อนปรน (Half-Lambert) เพื่อไม่ให้ด้านหลังทึบมืด
                half halfLambert = NdotL * 0.5 + 0.5;
                half toonLight = smoothstep(0.3, 0.35, halfLambert);

                // ดึงแสงรอบทิศทางใน Scene มาผสม ป้องกันเงาดำตึ๊ดตื๋อ
                half3 ambientLight = SampleSH(flatNormal);

                // ผสมสีแสงและเงา
                half3 litColor = lerp(_ShadowColor.rgb * baseAlbedo, baseAlbedo, toonLight);
                half3 finalColor = litColor * (mainLight.color + ambientLight);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}