Shader "TextMeshPro/StopMotionDistortion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Face Tint", Color) = (1,1,1,1)

        [Header(Distortion)]
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _DistortionStrength ("Distortion Strength", Range(0,0.2)) = 0.03
        _NormalTiling ("Normal Tiling", Vector) = (2,2,0,0)
        _NormalSpeed ("Normal Speed", Vector) = (0.15,0.08,0,0)

        [Header(Stop Motion)]
        _JitterFPS ("Jitter FPS", Range(1,60)) = 8
        _JitterAmount ("Warp Jitter", Range(0,0.2)) = 0.025
        _PositionJitter ("Position Jitter", Range(0,0.05)) = 0.005

        [Header(Frame Rotation)]
        _RotationJitter ("Rotation Jitter", Range(0,10)) = 1.0

        [Header(Frame Scale)]
        _ScaleJitter ("Scale Jitter", Range(0,0.1)) = 0.01

        [Header(SDF)]
        _OutlineWidth ("Outline Width", Range(0,1)) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)

        [Header(Alpha Cut)]
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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

            sampler2D _MainTex;
            sampler2D _NormalMap;

            float4 _MainTex_ST;

            fixed4 _FaceColor;
            fixed4 _OutlineColor;

            float _DistortionStrength;
            float4 _NormalTiling;
            float4 _NormalSpeed;

            float _JitterFPS;
            float _JitterAmount;
            float _PositionJitter;

            float _RotationJitter;
            float _ScaleJitter;

            float _OutlineWidth;
            float _AlphaCutoff;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;

                float2 uv : TEXCOORD0;
                float2 normalUV : TEXCOORD1;
            };

            float Random(float2 p)
            {
                return frac(
                    sin(
                        dot(
                            p,
                            float2(12.9898, 78.233)
                        )
                    ) * 43758.5453
                );
            }

            v2f vert(appdata_t v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normalUV = v.texcoord * _NormalTiling.xy;

                // ???????????: ??????????? Vertex (TextMeshPro GUI) ????? Fragment
                o.color = v.color * _FaceColor;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float frameID =
                    floor(
                        _Time.y *
                        _JitterFPS
                    );

                float frameTime =
                    frameID /
                    _JitterFPS;

                float2 normalUV =
                    i.normalUV +
                    frameTime *
                    _NormalSpeed.xy;

                fixed4 normalSample =
                    tex2D(
                        _NormalMap,
                        normalUV
                    );

                float2 distortion =
                    normalSample.rg * 2.0 - 1.0;

                float2 uv =
                    i.uv +
                    distortion *
                    _DistortionStrength;

                float2 randomPosition =
                    float2(
                        Random(
                            float2(
                                frameID,
                                12.34
                            )
                        ),
                        Random(
                            float2(
                                frameID,
                                56.78
                            )
                        )
                    );

                randomPosition =
                    randomPosition *
                    2.0 -
                    1.0;

                uv +=
                    randomPosition *
                    _PositionJitter;

                float2 center =
                    uv - 0.5;

                float randomRotation =
                    Random(
                        float2(
                            frameID,
                            91.42
                        )
                    );

                float angle =
                    (
                        randomRotation *
                        2.0 -
                        1.0
                    ) *
                    _RotationJitter *
                    0.0174532925;

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

                float randomScale =
                    Random(
                        float2(
                            frameID,
                            43.29
                        )
                    );

                float scale =
                    1.0 +
                    (
                        randomScale *
                        2.0 -
                        1.0
                    ) *
                    _ScaleJitter;

                rotated *= scale;

                uv =
                    rotated +
                    0.5;

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

                float2 extraWobble =
                    float2(
                        Random(
                            float2(
                                frameID,
                                72.1
                            )
                        ),
                        Random(
                            float2(
                                frameID,
                                38.7
                            )
                        )
                    );

                extraWobble =
                    extraWobble *
                    2.0 -
                    1.0;

                uv +=
                    extraWobble *
                    _PositionJitter *
                    0.35;

                if (uv.x < 0 ||
                    uv.x > 1 ||
                    uv.y < 0 ||
                    uv.y > 1)
                {
                    discard;
                }

                fixed4 tex =
                    tex2D(
                        _MainTex,
                        uv
                    );

                float sdf =
                    tex.a;

                float alpha =
                    smoothstep(
                        0.45,
                        0.55,
                        sdf
                    );

                alpha =
                    saturate(
                        alpha -
                        _AlphaCutoff
                    );

                clip(
                    alpha -
                    0.001
                );

                fixed4 col;

                // ???????? Vertex (i.color) ??????????? TextMeshPro GUI ??????
                col.rgb = i.color.rgb;
                col.a = alpha * i.color.a;

                return col;
            }

            ENDCG
        }
    }
}