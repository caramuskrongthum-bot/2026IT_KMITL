Shader "Custom/URP_VHS_CRT"
{
    Properties
    {
        _VHSIntensity ("VHS Intensity", Range(0,1)) = 0.7
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.3
        _ScanlineCount ("Scanline Count", Float) = 500

        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.08
        _StaticStrength ("Static Strength", Range(0,1)) = 0.04

        _TrackingStrength ("Tracking Error", Range(0,1)) = 0.15
        _TrackingSpeed ("Tracking Speed", Range(0,20)) = 4

        _GlitchStrength ("Glitch", Range(0,1)) = 0.15
        _GlitchSpeed ("Glitch Speed", Range(0,20)) = 8

        _RGBSplit ("RGB Separation", Range(0,0.02)) = 0.003

        _Curvature ("CRT Curvature", Range(0,0.2)) = 0.035
        _Vignette ("Vignette", Range(0,1)) = 0.4

        _ColorBleed ("Color Bleed", Range(0,0.02)) = 0.003

        _Brightness ("Brightness", Range(0.5,2)) = 1
        _Contrast ("Contrast", Range(0.5,2)) = 1.05
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "VHS"

            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            CBUFFER_START(UnityPerMaterial)

                float _VHSIntensity;
                float _ScanlineStrength;
                float _ScanlineCount;

                float _NoiseStrength;
                float _StaticStrength;

                float _TrackingStrength;
                float _TrackingSpeed;

                float _GlitchStrength;
                float _GlitchSpeed;

                float _RGBSplit;

                float _Curvature;
                float _Vignette;

                float _ColorBleed;

                float _Brightness;
                float _Contrast;

            CBUFFER_END


            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS = GetFullScreenTriangleVertexPosition(
                    input.vertexID
                );

                output.uv = GetFullScreenTriangleTexCoord(
                    input.vertexID
                );

                return output;
            }


            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }


            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }


            float3 ApplyCurvature(float2 uv)
            {
                float2 p = uv * 2.0 - 1.0;

                float r2 = dot(p, p);

                p *= 1.0 + r2 * _Curvature;

                return float3(
                    p * 0.5 + 0.5,
                    r2
                );
            }


            float VHSWave(float y)
            {
                float time = _Time.y;

                float wave =
                    sin(
                        y * 40.0 +
                        time * _TrackingSpeed
                    );

                wave +=
                    sin(
                        y * 93.0 -
                        time * 7.0
                    ) * 0.35;

                return wave * 0.0015;
            }


            float Tracking(float2 uv)
            {
                float time = _Time.y;

                float band =
                    sin(
                        time * _TrackingSpeed
                    ) * 0.5 + 0.5;

                float distanceToBand =
                    abs(
                        uv.y - band
                    );

                float mask =
                    1.0 -
                    smoothstep(
                        0.0,
                        0.025,
                        distanceToBand
                    );

                float distortion =
                    sin(
                        uv.y * 900.0 +
                        time * 30.0
                    );

                return distortion *
                       mask *
                       _TrackingStrength *
                       0.025;
            }


            float Glitch(float2 uv)
            {
                float time =
                    floor(
                        _Time.y *
                        _GlitchSpeed
                    );

                float n =
                    Hash21(
                        float2(
                            time,
                            floor(uv.y * 40.0)
                        )
                    );

                float active =
                    step(
                        0.88,
                        n
                    );

                float offset =
                    (n - 0.5) *
                    _GlitchStrength *
                    0.04;

                return offset * active;
            }


            float3 SampleVHS(float2 uv)
            {
                float time = _Time.y;

                float wave =
                    VHSWave(uv.y);

                float tracking =
                    Tracking(uv);

                float glitch =
                    Glitch(uv);

                float2 distortedUV = uv;

                distortedUV.x +=
                    wave +
                    tracking +
                    glitch;

                distortedUV.x +=
                    sin(
                        uv.y * 17.0 +
                        time * 2.0
                    ) *
                    0.001 *
                    _VHSIntensity;


                float2 rgbUV =
                    distortedUV;

                float split =
                    _RGBSplit *
                    _VHSIntensity;


                float3 col;

                col.r =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        rgbUV + float2(split, 0)
                    ).r;

                col.g =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        rgbUV
                    ).g;

                col.b =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        rgbUV - float2(split, 0)
                    ).b;


                float bleed =
                    _ColorBleed *
                    _VHSIntensity;

                float3 left =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        rgbUV - float2(bleed, 0)
                    ).rgb;

                float3 right =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_BlitTexture,
                        rgbUV + float2(bleed, 0)
                    ).rgb;

                col =
                    lerp(
                        col,
                        (left + col + right) / 3.0,
                        0.25
                    );


                return col;
            }


            float3 ApplyScanlines(
                float3 color,
                float2 uv
            )
            {
                float scan =
                    sin(
                        uv.y *
                        _ScanlineCount *
                        3.14159
                    );

                scan =
                    scan * 0.5 + 0.5;

                color *=
                    lerp(
                        1.0,
                        scan,
                        _ScanlineStrength
                    );

                return color;
            }


            float3 ApplyNoise(
                float3 color,
                float2 uv
            )
            {
                float time =
                    floor(
                        _Time.y * 60.0
                    );

                float n =
                    Hash21(
                        uv *
                        900.0 +
                        time
                    );

                color +=
                    (n - 0.5) *
                    _NoiseStrength;

                return color;
            }


            float3 ApplyStatic(
                float3 color,
                float2 uv
            )
            {
                float time =
                    floor(
                        _Time.y * 30.0
                    );

                float n =
                    Noise(
                        uv *
                        800.0 +
                        time
                    );

                float staticMask =
                    step(
                        0.94,
                        n
                    );

                color =
                    lerp(
                        color,
                        float3(n, n, n),
                        staticMask *
                        _StaticStrength
                    );

                return color;
            }


            float3 ApplyVignette(
                float3 color,
                float2 uv
            )
            {
                float2 p =
                    uv * 2.0 - 1.0;

                float d =
                    dot(p, p);

                float vignette =
                    1.0 -
                    d *
                    _Vignette;

                vignette =
                    saturate(vignette);

                return color * vignette;
            }


            float3 ApplyColor(
                float3 color
            )
            {
                color *= _Brightness;

                color =
                    (color - 0.5) *
                    _Contrast +
                    0.5;

                return saturate(color);
            }


            half4 Frag(Varyings input)
                : SV_Target
            {
                float2 uv =
                    input.uv;

                float3 curved =
                    ApplyCurvature(uv);

                float2 screenUV =
                    curved.xy;


                float3 color;

                if (
                    screenUV.x < 0.0 ||
                    screenUV.x > 1.0 ||
                    screenUV.y < 0.0 ||
                    screenUV.y > 1.0
                )
                {
                    color = float3(
                        0.0,
                        0.0,
                        0.0
                    );
                }
                else
                {
                    color =
                        SampleVHS(
                            screenUV
                        );
                }


                color =
                    ApplyScanlines(
                        color,
                        screenUV
                    );

                color =
                    ApplyNoise(
                        color,
                        screenUV
                    );

                color =
                    ApplyStatic(
                        color,
                        screenUV
                    );

                color =
                    ApplyVignette(
                        color,
                        screenUV
                    );

                color =
                    ApplyColor(
                        color
                    );


                return half4(
                    color,
                    1.0
                );
            }

            ENDHLSL
        }
    }
}