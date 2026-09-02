Shader "Custom/URP_NoiseDistortion"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _DistortionStrength ("Distortion Strength", Range(0, 0.2)) = 0.05
        _Speed ("Wave Speed (XY)", Vector) = (0.1, 0.1, 0, 0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float2 noiseUV      : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);       SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float _DistortionStrength;
                float2 _Speed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                // คำนวณ UV ของ MainTex และ NoiseTex
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                // เพิ่มการขยับตามเวลา (_Time.y) ให้กับ Noise UV เพื่อสร้างภาพเคลื่อนไหว
                output.noiseUV = TRANSFORM_TEX(input.uv, _NoiseTex) + (_Speed * _Time.y);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. อ่านค่าจาก Noise Texture
                float4 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.noiseUV);

                // 2. ปรับช่วงค่า Noise จาก [0, 1] เป็น [-1, 1] เพื่อให้เกิดการหักเหทั้งซ้าย-ขวา/บน-ล่าง
                float2 offset = (noise.rg * 2.0 - 1.0) * _DistortionStrength;

                // 3. นำค่า Offset ไปบวกเข้ากับ UV ของ Texture หลัก
                float2 distortedUV = input.uv + offset;

                // 4. Sample ภาพ Texture หลักด้วย UV ที่ถูกหักเหแล้ว
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);

                return color;
            }
            ENDHLSL
        }
    }
}