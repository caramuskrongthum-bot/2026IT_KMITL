Shader "Custom/Sprite_PixelExtrude"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _Thickness ("Extrude Thickness", Range(0, 0.5)) = 0.05

        _Steps ("Extrude Steps", Range(1, 32)) = 8

        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        _SideColor ("Side Color", Color) = (0.7, 0.7, 0.7, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }

        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #pragma target 4.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float side : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Thickness;
            float _Steps;
            float _Cutoff;

            float4 _SideColor;

            v2g vert(appdata v)
            {
                v2g o;

                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            [maxvertexcount(96)]
            void geom(
                triangle v2g input[3],
                inout TriangleStream<g2f> stream
            )
            {
                g2f o;

                int steps = (int)_Steps;

                for (int s = steps; s >= 0; s--)
                {
                    float t = (float)s / (float)steps;

                    float z = t * _Thickness;

                    for (int i = 0; i < 3; i++)
                    {
                        float4 pos = input[i].vertex;

                        pos.z += z;

                        o.pos = UnityObjectToClipPos(pos);
                        o.uv = input[i].uv;

                        o.side = (s == 0) ? 0 : 1;

                        stream.Append(o);
                    }

                    stream.RestartStrip();
                }
            }

            fixed4 frag(g2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                clip(col.a - _Cutoff);

                if (i.side > 0.5)
                {
                    col.rgb *= _SideColor.rgb;
                }

                return col;
            }

            ENDCG
        }
    }
}