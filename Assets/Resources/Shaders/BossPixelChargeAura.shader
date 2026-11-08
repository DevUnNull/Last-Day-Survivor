Shader "Custom/BossPixelChargeAura"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Charge Aura and Pixel Outline)]
        _ChargeAmount ("Muc Do Van Chieu (0-1)", Range(0, 1)) = 0.0
        _OutlineColor ("Mau Vien Nang Luong Ngoai", Color) = (1, 0.45, 0.1, 1)
        _InnerGlowColor ("Mau Loi Nang Luong Trong", Color) = (1, 0.95, 0.6, 1)
        _OutlineThickness ("Do Day Vien Pixel", Range(0, 4)) = 1.5
        _ScrollSpeed ("Toc Do Cuon Song Nang Luong", Range(0, 20)) = 8.0
        _WaveAmplitude ("Bien Do Rung Dinh (Sine Wave)", Range(0, 0.08)) = 0.015

        [Header(2 Tone Pixel Dissolve)]
        _DissolveAmount ("Muc Do Tan Bien (0-1)", Range(0, 1)) = 0.0
        _EdgeWidth ("Do Rong Vien Tan Bien", Range(0.01, 0.25)) = 0.09
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;

            float _ChargeAmount;
            fixed4 _OutlineColor;
            fixed4 _InnerGlowColor;
            float _OutlineThickness;
            float _ScrollSpeed;
            float _WaveAmplitude;

            float _DissolveAmount;
            float _EdgeWidth;

            // Procedural pixel-art hash noise (no external texture required)
            float PixelHash(float2 p)
            {
                float2 grid = floor(p * 48.0);
                return frac(sin(dot(grid, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                float4 pos = IN.vertex;

                // Sine Vertex Displacement from unity-shaders skill when charging
                if (_ChargeAmount > 0.01)
                {
                    float wave = sin(_Time.y * 28.0 + pos.y * 12.0) * _WaveAmplitude * _ChargeAmount;
                    pos.x += wave;
                }

                OUT.vertex = UnityObjectToClipPos(pos);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                fixed4 c = tex2D(_MainTex, uv) * IN.color;

                // 1. 2-Tone Pixel Dissolve (when Boss dies)
                if (_DissolveAmount > 0.001)
                {
                    float noise = PixelHash(uv);
                    float dissolve = noise - _DissolveAmount;
                    if (dissolve < 0.0)
                    {
                        discard;
                    }
                    if (dissolve < _EdgeWidth && c.a > 0.05)
                    {
                        float edgeFactor = saturate(dissolve / _EdgeWidth);
                        fixed4 edgeCol = lerp(_OutlineColor, _InnerGlowColor, 1.0 - edgeFactor);
                        c.rgb = edgeCol.rgb;
                    }
                }

                // 2. Pixel-Art Camera/Sprite Outline & Scrolling Energy Aura during Vận Chiêu (_ChargeAmount > 0)
                if (_ChargeAmount > 0.001)
                {
                    float2 texel = _MainTex_TexelSize.xy * (_OutlineThickness * (0.8 + 0.6 * _ChargeAmount));

                    float neighborAlphaMax = 0.0;
                    float neighborAlphaMin = 1.0;

                    float aU = tex2D(_MainTex, uv + float2(0.0, texel.y)).a;
                    float aD = tex2D(_MainTex, uv - float2(0.0, texel.y)).a;
                    float aL = tex2D(_MainTex, uv - float2(texel.x, 0.0)).a;
                    float aR = tex2D(_MainTex, uv + float2(texel.x, 0.0)).a;

                    float aUL = tex2D(_MainTex, uv + float2(-texel.x, texel.y)).a;
                    float aUR = tex2D(_MainTex, uv + float2(texel.x, texel.y)).a;
                    float aDL = tex2D(_MainTex, uv + float2(-texel.x, -texel.y)).a;
                    float aDR = tex2D(_MainTex, uv + float2(texel.x, -texel.y)).a;

                    neighborAlphaMax = max(max(aU, aD), max(aL, aR));
                    neighborAlphaMax = max(neighborAlphaMax, max(max(aUL, aUR), max(aDL, aDR)));

                    neighborAlphaMin = min(min(aU, aD), min(aL, aR));

                    // Rising energy band (Scrolling UV technique)
                    float scrollBand = sin((uv.y * 24.0) - (_Time.y * _ScrollSpeed)) * 0.5 + 0.5;
                    float pulse = 0.65 + 0.35 * sin(_Time.y * 18.0);

                    // Outer Pixel Outline on transparent pixels bordering the Boss sprite
                    if (c.a < 0.1 && neighborAlphaMax > 0.2)
                    {
                        fixed4 outCol = lerp(_OutlineColor, _InnerGlowColor, scrollBand * 0.6);
                        outCol.a = saturate(_ChargeAmount * 1.25 * pulse);
                        return outCol;
                    }

                    // Inner Energy Rim & Charge Surge on opaque Boss pixels
                    if (c.a >= 0.1)
                    {
                        bool isInnerRim = (neighborAlphaMin < 0.2);
                        float rimBoost = isInnerRim ? 0.55 : 0.0;
                        float bandBoost = step(0.72, scrollBand) * 0.35;
                        float totalGlow = saturate((_ChargeAmount * 0.55) + (rimBoost * _ChargeAmount) + (bandBoost * _ChargeAmount));

                        fixed3 glowRgb = lerp(_OutlineColor.rgb, _InnerGlowColor.rgb, isInnerRim ? 0.75 : scrollBand);
                        c.rgb = lerp(c.rgb, glowRgb, totalGlow);
                    }
                }

                return c;
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
