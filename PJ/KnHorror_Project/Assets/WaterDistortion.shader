Shader "UI/RawImageStopMotionDistortion"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Raw Image Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)

        // =========================================================
        // DISTORTION
        // =========================================================

        [Header(Distortion)]

        _NormalMap ("Normal Map", 2D) = "bump" {}

        _DistortionStrength
        ("Distortion Strength", Range(0, 0.2)) = 0.04

        _NormalTiling
        ("Normal Tiling", Vector) = (2, 2, 0, 0)

        _NormalSpeed
        ("Normal Speed", Vector) = (0.15, 0.08, 0, 0)


        // =========================================================
        // STOP MOTION
        // =========================================================

        [Header(Stop Motion)]

        _JitterFPS
        ("Jitter FPS", Range(1, 60)) = 8

        _JitterAmount
        ("Warp Jitter", Range(0, 0.2)) = 0.035

        _PositionJitter
        ("Position Jitter", Range(0, 0.05)) = 0.008


        // =========================================================
        // FRAME ROTATION
        // =========================================================

        [Header(Frame Rotation)]

        _RotationJitter
        ("Rotation Jitter", Range(0, 10)) = 1.5


        // =========================================================
        // FRAME SCALE
        // =========================================================

        [Header(Frame Scale)]

        _ScaleJitter
        ("Scale Jitter", Range(0, 0.1)) = 0.015


        // =========================================================
        // ALPHA CUT
        // =========================================================

        [Header(Alpha Cut)]

        _AlphaCutoff
        ("Alpha Cutoff", Range(0, 1)) = 0.1


        // =========================================================
        // EDGE
        // =========================================================

        [Header(Edge)]

        _ClampUV
        ("Clamp UV", Range(0, 1)) = 1
    }


    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "CanUseSpriteAtlas" = "True"
        }


        Cull Off
        Lighting Off
        ZWrite Off

        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"


            // =====================================================
            // TEXTURES
            // =====================================================

            sampler2D _MainTex;
            sampler2D _NormalMap;


            // =====================================================
            // PARAMETERS
            // =====================================================

            float4 _Color;

            float _DistortionStrength;

            float4 _NormalTiling;
            float4 _NormalSpeed;

            float _JitterFPS;

            float _JitterAmount;
            float _PositionJitter;

            float _RotationJitter;
            float _ScaleJitter;

            float _AlphaCutoff;

            float _ClampUV;


            // =====================================================
            // STRUCTS
            // =====================================================

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };


            struct v2f
            {
                float4 vertex   : SV_POSITION;

                fixed4 color    : COLOR;

                float2 uv       : TEXCOORD0;

                float2 normalUV : TEXCOORD1;
            };


            // =====================================================
            // RANDOM
            // =====================================================

            float Random(float2 p)
            {
                return frac(
                    sin(
                        dot(
                            p,
                            float2(
                                12.9898,
                                78.233
                            )
                        )
                    )
                    * 43758.5453
                );
            }


            // =====================================================
            // VERTEX
            // =====================================================

            v2f vert(appdata_t v)
            {
                v2f o;

                o.vertex =
                    UnityObjectToClipPos(v.vertex);

                o.uv =
                    v.texcoord;

                o.normalUV =
                    v.texcoord *
                    _NormalTiling.xy;

                o.color =
                    v.color *
                    _Color;

                return o;
            }


            // =====================================================
            // FRAGMENT
            // =====================================================

            fixed4 frag(v2f i) : SV_Target
            {

                // =================================================
                // CREATE DISCRETE STOP-MOTION FRAMES
                // =================================================

                float frameID =
                    floor(
                        _Time.y *
                        _JitterFPS
                    );


                float frameTime =
                    frameID /
                    _JitterFPS;


                // =================================================
                // NORMAL MAP
                // =================================================

                float2 normalUV =
                    i.normalUV +
                    frameTime *
                    _NormalSpeed.xy;


                fixed4 normalSample =
                    tex2D(
                        _NormalMap,
                        normalUV
                    );


                // =================================================
                // NORMAL → DISTORTION
                // =================================================

                float2 distortion;

                distortion.x =
                    normalSample.r *
                    2.0 -
                    1.0;

                distortion.y =
                    normalSample.g *
                    2.0 -
                    1.0;


                // =================================================
                // BASIC DISTORTION
                // =================================================

                float2 uv =
                    i.uv +
                    distortion *
                    _DistortionStrength;


                // =================================================
                // RANDOM POSITION JUMP
                // =================================================

                float randomX =
                    Random(
                        float2(
                            frameID,
                            13.71
                        )
                    )
                    * 2.0 -
                    1.0;


                float randomY =
                    Random(
                        float2(
                            frameID,
                            51.37
                        )
                    )
                    * 2.0 -
                    1.0;


                float2 positionJitter =
                    float2(
                        randomX,
                        randomY
                    )
                    *
                    _PositionJitter;


                uv += positionJitter;


                // =================================================
                // CENTER
                // =================================================

                float2 center =
                    uv -
                    0.5;


                // =================================================
                // RANDOM ROTATION
                // =================================================

                float randomRotation =
                    Random(
                        float2(
                            frameID,
                            91.42
                        )
                    )
                    * 2.0 -
                    1.0;


                float angle =
                    radians(
                        randomRotation *
                        _RotationJitter
                    );


                float sinAngle =
                    sin(angle);

                float cosAngle =
                    cos(angle);


                float2 rotated;

                rotated.x =
                    center.x *
                    cosAngle -
                    center.y *
                    sinAngle;

                rotated.y =
                    center.x *
                    sinAngle +
                    center.y *
                    cosAngle;


                // =================================================
                // RANDOM SCALE
                // =================================================

                float randomScale =
                    Random(
                        float2(
                            frameID,
                            43.29
                        )
                    )
                    * 2.0 -
                    1.0;


                float scale =
                    1.0 +
                    randomScale *
                    _ScaleJitter;


                rotated *= scale;


                uv =
                    rotated +
                    0.5;


                // =================================================
                // EXTRA WARP
                // =================================================

                float distanceFromCenter =
                    length(
                        uv - 0.5
                    );


                float warpStrength =
                    _JitterAmount *
                    distanceFromCenter;


                uv +=
                    distortion *
                    warpStrength;


                // =================================================
                // EXTRA FRAME-SKIP WOBBLE
                // =================================================

                float wobbleX =
                    Random(
                        float2(
                            frameID,
                            72.19
                        )
                    )
                    * 2.0 -
                    1.0;


                float wobbleY =
                    Random(
                        float2(
                            frameID,
                            21.83
                        )
                    )
                    * 2.0 -
                    1.0;


                uv +=
                    float2(
                        wobbleX,
                        wobbleY
                    )
                    *
                    _PositionJitter *
                    0.35;


                // =================================================
                // CLAMP UV
                // =================================================

                if (_ClampUV > 0.5)
                {
                    uv =
                        saturate(uv);
                }


                // =================================================
                // SAMPLE MAIN TEXTURE
                // =================================================

                fixed4 col =
                    tex2D(
                        _MainTex,
                        uv
                    );


                // =================================================
                // APPLY UI COLOR
                // =================================================

                col *= i.color;


                // =================================================
                // ALPHA CUT
                // =================================================

                clip(
                    col.a -
                    _AlphaCutoff
                );


                // =================================================
                // OUTPUT
                // =================================================

                return col;
            }

            ENDCG
        }
    }
}