Shader "Custom/GhostScreenProjectorShake"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (0,1,0,1)
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.3
        _Speed ("Speed", Range(1, 20)) = 10
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                // ไม่ใช้ UV เดิม (Texture อิงมุมมองกล้อง)
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0; // เก็บพิกัดหน้าจอ
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _GlitchIntensity;
            float _Speed;

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                // --- ส่วนที่เพิ่มมา: ทำให้ตัวละครสั่น ---
                float time = _Time.y * _Speed;
                
                // คำนวณตำแหน่งจอของ Vertex (Clip Space) เพื่อเอาไปคำนวณการสั่น
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                
                // สร้างค่าสั่นแบบบล็อกๆ ตามเวลาและตำแหน่งจอ
                float2 shakeOffset = (random(floor(clipPos.xy * 50.0 + time)) - 0.5) * _GlitchIntensity * 0.1;
                
                // ใส่ค่าสั่นเข้าไปที่ตำแหน่งจอ X และ Y (ทำให้สั่นไปมาบนจอ)
                clipPos.xy += shakeOffset;
                
                o.vertex = clipPos;
                o.screenPos = ComputeScreenPos(clipPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;
                
                // แปลงพิกัดหน้าจอเป็น UV
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // สร้างบล็อก Glitch ตามพิกัดหน้าจอ
                float noiseBlock = random(floor(screenUV * 30.0 + time));
                
                // ใช้ screenUV เป็น Texture Coordinate
                float2 uv = screenUV;

                // ถ้าจังหวะบัคมา ให้ดึงพิกัดหน้าจอเพี้ยนตามแนวขวาง
                if (noiseBlock > (1.0 - _GlitchIntensity * 0.5))
                {
                    uv.x += (random(float2(time, screenUV.y)) - 0.5) * _GlitchIntensity * 0.3;
                }

                // RGB Split
                float splitAmount = _GlitchIntensity * 0.04 * noiseBlock;

                fixed4 colR = tex2D(_MainTex, uv + float2(splitAmount, 0));
                fixed4 colG = tex2D(_MainTex, uv);
                fixed4 colB = tex2D(_MainTex, uv - float2(splitAmount, 0));

                fixed4 finalCol;
                finalCol.r = colR.r;
                finalCol.g = colG.g;
                finalCol.b = colB.b;
                finalCol.a = (colR.a + colG.a + colB.a) / 3.0;

                // สแกนไลน์
                float scanline = sin(screenUV.y * 500.0 + time * 15.0) * 0.06 * _GlitchIntensity;
                finalCol.rgb -= scanline;

                // ย้อมสี
                finalCol *= _Color;

                // กระพริบหาย
                if (random(float2(time, 4.0)) > 0.93)
                {
                    finalCol.a *= 0.05;
                }

                return finalCol;
            }
            ENDCG
        }
    }
}