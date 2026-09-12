Shader "Custom/URP_RainDropsTransparentGlass"
{
    Properties
    {
        _BaseColor ("Glass Tint Color", Color) = (1,1,1,0.3)
        _NormalMap1 ("Rain Normal 1 (Fast)", 2D) = "bump" {}
        _NormalMap2 ("Rain Normal 2 (Slow)", 2D) = "bump" {}
        _Speed1 ("Flow Speed 1", Float) = 0.8
        _Speed2 ("Flow Speed 2", Float) = 0.2
        _NormalIntensity ("Normal Intensity", Range(0, 3)) = 1.0
        _Smoothness ("Specular Smoothness", Range(0, 1)) = 0.9
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // เปิด Blending ให้โปร่งใสแบบ Alpha Blend
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float3 tangentWS    : TEXCOORD3;
                float3 bitangentWS  : TEXCOORD4;
            };

            Texture2D _NormalMap1;
            SamplerState sampler_NormalMap1;

            Texture2D _NormalMap2;
            SamplerState sampler_NormalMap2;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _NormalMap1_ST;
                float4 _NormalMap2_ST;
                float _Speed1;
                float _Speed2;
                float _NormalIntensity;
                float _Smoothness;
                float _SpecularIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;

                output.uv = TRANSFORM_TEX(input.uv, _NormalMap1);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // เลื่อน UV สายฝนสองชั้น
                float2 uv1 = input.uv;
                uv1.y += _Time.y * _Speed1;

                float2 uv2 = input.uv;
                uv2.y += _Time.y * _Speed2;

                // Sample Normal Map ทั้งสองชั้น
                float4 normTex1 = _NormalMap1.Sample(sampler_NormalMap1, uv1);
                float4 normTex2 = _NormalMap2.Sample(sampler_NormalMap2, uv2);

                float3 localNormal1 = UnpackNormalScale(normTex1, _NormalIntensity);
                float3 localNormal2 = UnpackNormalScale(normTex2, _NormalIntensity);
                float3 blendedLocalNormal = normalize(localNormal1 + localNormal2);

                // แปลง Normal เป็น World Space
                float3 T = normalize(input.tangentWS);
                float3 B = normalize(input.bitangentWS);
                float3 N = normalize(input.normalWS);
                float3 normalWS = TransformTangentToWorld(blendedLocalNormal, half3x3(T, B, N));

                // ดึงข้อมูลแสงหลัก (Main Light) ใน URP
                Light mainLight = GetMainLight();
                float3 viewDirWS = GetWorldSpaceViewDir(input.positionWS);

                // คำนวณ Specular Highlight ให้หยดน้ำมีความเงาวับสะท้อนแสงไฟ
                float3 halfDir = normalize(mainLight.direction + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specular = pow(NdotH, _Smoothness * 128.0) * _SpecularIntensity;

                // รวมสีของกระจกเข้ากับไฮไลท์แสงสะท้อน
                float3 finalRGB = _BaseColor.rgb + (mainLight.color * specular);
                float finalAlpha = _BaseColor.a + (specular * 0.5); // ให้ตรงไฮไลท์มีความทึบขึ้นนิดนึงเพื่อความสมจริง

                return float4(finalRGB, saturate(finalAlpha));
            }
            ENDHLSL
        }
    }
}