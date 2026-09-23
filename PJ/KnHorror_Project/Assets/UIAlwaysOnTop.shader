Shader "Custom/UIBillboardAlwaysOnTop"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)

        _FadeStart ("Fade Start Distance", Float) = 15
        _FadeEnd ("Fade End Distance", Float) = 25
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent+100"
            "RenderPipeline"="UniversalPipeline"
        }

        LOD 100

        ZTest Always
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
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;

                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseMap_ST;
                float4 _BaseColor;

                float _FadeStart;
                float _FadeEnd;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 pivotWS = TransformObjectToWorld(float3(0, 0, 0));

                float3 toCamera =
                    normalize(GetCameraPositionWS() - pivotWS);

                // Billboard
                float3 forward =
                    normalize(float3(toCamera.x, 0, toCamera.z));

                float3 right =
                    normalize(cross(float3(0, 1, 0), forward));

                float3 up = float3(0, 1, 0);

                // World Scale
                float3 scaleWS = float3(
                    length(UNITY_MATRIX_M._m00_m10_m20),
                    length(UNITY_MATRIX_M._m01_m11_m21),
                    length(UNITY_MATRIX_M._m02_m12_m22)
                );

                float3 finalWorldPos =
                    pivotWS
                    + right * input.positionOS.x * scaleWS.x
                    + up * input.positionOS.y * scaleWS.y;

                output.positionCS =
                    TransformWorldToHClip(finalWorldPos);

                output.uv =
                    TRANSFORM_TEX(input.uv, _BaseMap);

                output.color =
                    input.color * _BaseColor;

                // เก็บตำแหน่ง World Space
                output.worldPos = finalWorldPos;

                return output;
            }


            float4 frag(Varyings input) : SV_Target
            {
                float4 texColor =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        input.uv
                    );

                float3 cameraPos =
                    GetCameraPositionWS();

                // ระยะจากกล้อง
                float distanceToCamera =
                    distance(input.worldPos, cameraPos);

                // 0 = เห็นเต็ม
                // 1 = หายไป
                float fade =
                    smoothstep(
                        _FadeStart,
                        _FadeEnd,
                        distanceToCamera
                    );

                // กลับค่า
                float alpha =
                    1.0 - fade;

                float4 finalColor =
                    texColor * input.color;

                finalColor.a *= alpha;

                return finalColor;
            }

            ENDHLSL
        }
    }
}