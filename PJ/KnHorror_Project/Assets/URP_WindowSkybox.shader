Shader "Custom/URP_WindowSkybox"
{
    Properties
    {
        _Skybox ("Window Skybox", Cube) = "" {}

        _Tint ("Skybox Tint", Color) = (1,1,1,1)

        _Exposure ("Skybox Exposure", Range(0,3)) = 1.0

        _Rotation ("Skybox Rotation", Range(0,360)) = 0

        _Alpha ("Window Transparency", Range(0,1)) = 1.0

        // ระยะเฟดตามกล้อง
        _FadeStart ("Fade Start Distance", Float) = 5.0
        _FadeEnd ("Fade End Distance", Float) = 25.0

        _FogStrength ("Fog Strength", Range(0,1)) = 1.0
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
            Name "WindowSkybox"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float3 positionWS : TEXCOORD0;

                float3 viewDirWS : TEXCOORD1;

                float fogFactor : TEXCOORD2;

                float distToCamera : TEXCOORD3;
            };


            TEXTURECUBE(_Skybox);
            SAMPLER(sampler_Skybox);


            CBUFFER_START(UnityPerMaterial)

                float4 _Tint;

                float _Exposure;

                float _Rotation;

                float _Alpha;

                float _FadeStart;

                float _FadeEnd;

                float _FogStrength;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;


                VertexPositionInputs vertexInput =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );


                output.positionCS =
                    vertexInput.positionCS;


                output.positionWS =
                    vertexInput.positionWS;


                /*
                    View direction
                */

                output.viewDirWS =
                    GetWorldSpaceViewDir(
                        vertexInput.positionWS
                    );


                /*
                    URP Fog & Distance Calculation
                */

                float3 positionVS =
                    TransformWorldToView(
                        vertexInput.positionWS
                    );

                output.fogFactor =
                    ComputeFogFactor(
                        -positionVS.z
                    );

                // ระยะห่างจากกล้องตรงๆ (View Space Z เชิงบวก)
                output.distToCamera =
                    -positionVS.z;


                return output;
            }


            float3 RotateY(
                float3 direction,
                float angle
            )
            {
                float rad =
                    radians(angle);


                float s =
                    sin(rad);


                float c =
                    cos(rad);


                float3 rotated;


                rotated.x =
                    direction.x * c -
                    direction.z * s;


                rotated.y =
                    direction.y;


                rotated.z =
                    direction.x * s +
                    direction.z * c;


                return rotated;
            }


            float4 frag(Varyings input) : SV_Target
            {
                /*
                    ====================================
                    VIEW DIRECTION
                    ====================================
                */

                float3 viewDir =
                    normalize(
                        input.viewDirWS
                    );


                /*
                    ====================================
                    SKYBOX ROTATION
                    ====================================
                */

                /*
                    ====================================
                    SKYBOX ROTATION & FLIP FIX
                    ====================================
                */

                viewDir =
                    RotateY(
                        viewDir,
                        _Rotation
                    );

                // กลับด้านแกน Y ตรงนี้เพื่อให้ภาพ Cubemap หายกลับหัว
                viewDir.y = -viewDir.y;


                /*
                    ====================================
                    SAMPLE CUBEMAP
                    ====================================
                */

                float3 skyColor =
                    SAMPLE_TEXTURECUBE(
                        _Skybox,
                        sampler_Skybox,
                        viewDir
                    ).rgb;


                /*
                    ====================================
                    TINT
                    ====================================
                */

                skyColor *=
                    _Tint.rgb;


                /*
                    ====================================
                    EXPOSURE
                    ====================================
                */

                skyColor *=
                    _Exposure;


                /*
                    ====================================
                    DISTANCE FADE (เฟดหายตามระยะกล้อง)
                    ====================================
                */

                float distanceFade =
                    saturate(
                        (input.distToCamera - _FadeStart) /
                        (_FadeEnd - _FadeStart)
                    );

                // ยิ่งถอยไกล Alpha จะยิ่งลู่เข้าหา 0 (โปร่งใสหายไป)
                float finalAlpha =
                    _Alpha *
                    (1.0 - distanceFade);


                /*
                    ====================================
                    FOG
                    ====================================
                */

                float3 foggedColor =
                    MixFog(
                        skyColor,
                        input.fogFactor
                    );


                skyColor =
                    lerp(
                        skyColor,
                        foggedColor,
                        _FogStrength
                    );


                /*
                    ====================================
                    FINAL
                    ====================================
                */

                return float4(
                    skyColor,
                    finalAlpha
                );
            }

            ENDHLSL
        }
    }
}