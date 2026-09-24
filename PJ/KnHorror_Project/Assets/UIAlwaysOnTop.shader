Shader "Custom/UIBillboardAlwaysOnTop"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)

        _NearDistance ("Near Hide Distance", Float) = 3
        _MiddleDistance ("Middle Visible Distance", Float) = 5

        _FadeStart ("Far Fade Start", Float) = 15
        _FadeEnd ("Far Fade End", Float) = 25

        [Toggle] _EnableSpin ("Enable 360 Spin", Float) = 0
        _SpinSpeed ("Spin Speed", Float) = 100
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

                // Fade ของ Object นั้นๆ
                float fade : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseMap_ST;
                float4 _BaseColor;

                float _NearDistance;
                float _MiddleDistance;

                float _FadeStart;
                float _FadeEnd;

                float _EnableSpin;
                float _SpinSpeed;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                // =========================================
                // Object Pivot
                // =========================================

                float3 pivotWS =
                    TransformObjectToWorld(float3(0, 0, 0));

                float3 cameraPos =
                    GetCameraPositionWS();


                // =========================================
                // ระยะจาก Pivot -> Camera
                // =========================================

                float distanceToCamera =
                    distance(pivotWS, cameraPos);


                // =========================================
                // NEAR FADE
                //
                // ใกล้เกินไป = หาย
                // =========================================

                float nearFade =
                    smoothstep(
                        _NearDistance,
                        _MiddleDistance,
                        distanceToCamera
                    );


                // =========================================
                // FAR FADE
                //
                // ไกลเกินไป = จาง
                // =========================================

                float farFade =
                    1.0 -
                    smoothstep(
                        _FadeStart,
                        _FadeEnd,
                        distanceToCamera
                    );


                output.fade =
                    nearFade * farFade;


                // =========================================
                // FULL 3D BILLBOARD
                // =========================================

                float3 toCamera =
                    normalize(cameraPos - pivotWS);

                float3 worldUp =
                    float3(0, 1, 0);

                float3 right =
                    normalize(
                        cross(worldUp, toCamera)
                    );


                // ป้องกันกล้องมองตรงขึ้น/ลง
                if (length(right) < 0.001)
                {
                    right = float3(1, 0, 0);
                }

                float3 up =
                    normalize(
                        cross(toCamera, right)
                    );


                // =========================================
                // SPIN
                // =========================================

                if (_EnableSpin > 0.5)
                {
                    float angle =
                        _Time.y *
                        _SpinSpeed *
                        (PI / 180.0);

                    float cosA = cos(angle);
                    float sinA = sin(angle);

                    float3 rotatedRight =
                        right * cosA -
                        up * sinA;

                    float3 rotatedUp =
                        right * sinA +
                        up * cosA;

                    right = rotatedRight;
                    up = rotatedUp;
                }


                // =========================================
                // World Scale
                // =========================================

                float3 scaleWS = float3(
                    length(UNITY_MATRIX_M._m00_m10_m20),
                    length(UNITY_MATRIX_M._m01_m11_m21),
                    length(UNITY_MATRIX_M._m02_m12_m22)
                );


                // =========================================
                // Billboard Position
                // =========================================

                float3 finalWorldPos =
                    pivotWS
                    + right *
                      input.positionOS.x *
                      scaleWS.x
                    + up *
                      input.positionOS.y *
                      scaleWS.y;


                output.positionCS =
                    TransformWorldToHClip(
                        finalWorldPos
                    );


                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _BaseMap
                    );


                output.color =
                    input.color *
                    _BaseColor;


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


                float4 finalColor =
                    texColor *
                    input.color;


                finalColor.a *= input.fade;


                return finalColor;
            }

            ENDHLSL
        }
    }
}