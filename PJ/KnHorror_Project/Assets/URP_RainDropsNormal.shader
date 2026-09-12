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
        _NormalBlend ("Normal Layer Blend", Range(0, 1)) = 0.5

        _Smoothness ("Specular Smoothness", Range(0, 1)) = 0.9
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.0

        _ReflectionStrength ("Reflection Strength", Range(0, 2)) = 0.5

        // ระยะเฟดตามกล้อง
        _FadeStart ("Fade Start Distance", Float) = 5.0
        _FadeEnd ("Fade End Distance", Float) = 25.0

        _FogStrength ("Fog Strength", Range(0, 1)) = 1.0
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float2 uv : TEXCOORD0;

                float3 positionWS : TEXCOORD1;

                float3 normalWS : TEXCOORD2;

                float3 tangentWS : TEXCOORD3;

                float3 bitangentWS : TEXCOORD4;

                float fogFactor : TEXCOORD5;

                float distToCamera : TEXCOORD6;
            };


            TEXTURE2D(_NormalMap1);
            SAMPLER(sampler_NormalMap1);

            TEXTURE2D(_NormalMap2);
            SAMPLER(sampler_NormalMap2);


            CBUFFER_START(UnityPerMaterial)

                float4 _BaseColor;

                float4 _NormalMap1_ST;
                float4 _NormalMap2_ST;

                float _Speed1;
                float _Speed2;

                float _NormalIntensity;
                float _NormalBlend;

                float _Smoothness;
                float _SpecularIntensity;

                float _ReflectionStrength;

                float _FadeStart;

                float _FadeEnd;

                float _FogStrength;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput =
                    GetVertexPositionInputs(input.positionOS.xyz);


                output.positionCS =
                    vertexInput.positionCS;


                output.positionWS =
                    vertexInput.positionWS;


                output.uv =
                    input.uv;


                VertexNormalInputs normalInput =
                    GetVertexNormalInputs(
                        input.normalOS,
                        input.tangentOS
                    );


                output.normalWS =
                    normalInput.normalWS;


                output.tangentWS =
                    normalInput.tangentWS;


                output.bitangentWS =
                    normalInput.bitangentWS;


                /*
                    ============================
                    URP FOG & DISTANCE
                    ============================
                */

                float3 positionVS =
                    TransformWorldToView(
                        vertexInput.positionWS
                    );

                output.fogFactor =
                    ComputeFogFactor(
                        -positionVS.z
                    );

                output.distToCamera =
                    -positionVS.z;


                return output;
            }


            float3 BlendRainNormals(
                float3 normal1,
                float3 normal2,
                float blend
            )
            {
                normal1.xy *= _NormalIntensity;

                normal2.xy *= _NormalIntensity;


                float3 result;


                result.xy =
                    lerp(
                        normal1.xy,
                        normal2.xy,
                        blend
                    );


                result.z =
                    sqrt(
                        saturate(
                            1.0 -
                            dot(
                                result.xy,
                                result.xy
                            )
                        )
                    );


                return normalize(result);
            }


            float4 frag(Varyings input) : SV_Target
            {
                /*
                    ========================================
                    RAIN UV
                    ========================================
                */

                float2 uv1 =
                    TRANSFORM_TEX(
                        input.uv,
                        _NormalMap1
                    );


                float2 uv2 =
                    TRANSFORM_TEX(
                        input.uv,
                        _NormalMap2
                    );


                /*
                    Rain movement
                */

                uv1.y +=
                    _Time.y *
                    _Speed1;


                uv2.y +=
                    _Time.y *
                    _Speed2;


                /*
                    ========================================
                    SAMPLE NORMAL MAP
                    ========================================
                */

                float4 rainTex1 =
                    SAMPLE_TEXTURE2D(
                        _NormalMap1,
                        sampler_NormalMap1,
                        uv1
                    );


                float4 rainTex2 =
                    SAMPLE_TEXTURE2D(
                        _NormalMap2,
                        sampler_NormalMap2,
                        uv2
                    );


                float3 normal1 =
                    UnpackNormal(
                        rainTex1
                    );


                float3 normal2 =
                    UnpackNormal(
                        rainTex2
                    );


                /*
                    ========================================
                    BLEND RAIN NORMAL
                    ========================================
                */

                float3 blendedNormal =
                    BlendRainNormals(
                        normal1,
                        normal2,
                        _NormalBlend
                    );


                /*
                    ========================================
                    TANGENT -> WORLD
                    ========================================
                */

                float3 T =
                    normalize(
                        input.tangentWS
                    );


                float3 B =
                    normalize(
                        input.bitangentWS
                    );


                float3 N =
                    normalize(
                        input.normalWS
                    );


                float3 normalWS =
                    TransformTangentToWorld(
                        blendedNormal,
                        half3x3(
                            T,
                            B,
                            N
                        )
                    );


                normalWS =
                    normalize(
                        normalWS
                    );


                /*
                    ========================================
                    VIEW DIRECTION
                    ========================================
                */

                float3 viewDirWS =
                    normalize(
                        GetWorldSpaceViewDir(
                            input.positionWS
                        )
                    );


                /*
                    ========================================
                    MAIN LIGHT
                    ========================================
                */

                Light mainLight =
                    GetMainLight();


                float3 lightDir =
                    normalize(
                        mainLight.direction
                    );


                /*
                    ========================================
                    SPECULAR
                    ========================================
                */

                float3 halfDir =
                    normalize(
                        lightDir +
                        viewDirWS
                    );


                float NdotH =
                    saturate(
                        dot(
                            normalWS,
                            halfDir
                        )
                    );


                float smoothnessPower =
                    lerp(
                        8.0,
                        256.0,
                        _Smoothness
                    );


                float specular =
                    pow(
                        NdotH,
                        smoothnessPower
                    );


                specular *=
                    _SpecularIntensity;


                /*
                    ========================================
                    FRESNEL
                    ========================================
                */

                float NdotV =
                    saturate(
                        dot(
                            normalWS,
                            viewDirWS
                        )
                    );


                float fresnel =
                    pow(
                        1.0 - NdotV,
                        4.0
                    );


                /*
                    ========================================
                    GLASS BASE
                    ========================================
                */

                float3 finalRGB =
                    _BaseColor.rgb;


                /*
                    ========================================
                    REFLECTION
                    ========================================
                */

                float3 reflectionColor =
                    mainLight.color *
                    fresnel *
                    _ReflectionStrength;


                finalRGB +=
                    reflectionColor;


                /*
                    ========================================
                    RAIN SPECULAR
                    ========================================
                */

                finalRGB +=
                    mainLight.color *
                    specular;


                /*
                    ========================================
                    ALPHA & DISTANCE FADE
                    ========================================
                */

                float finalAlpha =
                    _BaseColor.a;


                finalAlpha +=
                    specular *
                    0.5;


                finalAlpha +=
                    fresnel *
                    _ReflectionStrength *
                    0.15;


                // คำนวณความจางตามระยะกล้อง
                float distanceFade =
                    saturate(
                        (input.distToCamera - _FadeStart) /
                        (_FadeEnd - _FadeStart)
                    );

                finalAlpha *=
                    (1.0 - distanceFade);

                finalAlpha =
                    saturate(
                        finalAlpha
                    );


                /*
                    ========================================
                    FOG
                    ========================================
                */

                float3 foggedRGB =
                    MixFog(
                        finalRGB,
                        input.fogFactor
                    );


                /*
                    ========================================
                    FOG STRENGTH
                    ========================================
                */

                finalRGB =
                    lerp(
                        finalRGB,
                        foggedRGB,
                        _FogStrength
                    );


                /*
                    ========================================
                    FINAL
                    ========================================
                */

                return float4(
                    finalRGB,
                    finalAlpha
                );
            }

            ENDHLSL
        }
    }
}