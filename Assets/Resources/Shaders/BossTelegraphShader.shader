Shader "Custom/BossTelegraphShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1, 0.3, 0.1, 1)
        _CoreColor ("Core Highlight Color", Color) = (1, 0.95, 0.75, 1)
        _Progress ("Charge Progress (0-1)", Range(0, 1)) = 0.0
        _ShapeType ("0=Line, 1=Circle, 2=ThinRay", Float) = 0.0
        _WorldSize ("World Width & Length", Vector) = (2.0, 12.0, 0, 0)
        _PixelDensity ("Pixels Per Unit", Float) = 16.0
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
                float2 uv       : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _CoreColor;
            float _Progress;
            float _ShapeType;
            float4 _WorldSize;
            float _PixelDensity;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float ppu = max(8.0, _PixelDensity);
                float wWidth = max(0.2, _WorldSize.x);
                float wLength = max(0.2, _WorldSize.y);

                // Quantize UVs to uniform world-space pixel grid so pixels NEVER stretch!
                float2 worldPos = float2((IN.uv.x - 0.5) * wWidth, IN.uv.y * wLength);
                float2 pixWorld = floor(worldPos * ppu + 0.5) / ppu;

                // Normalized quantized coordinates: nx in [-0.5, 0.5], ny in [0, 1]
                float nx = pixWorld.x / wWidth;
                float ny = saturate(pixWorld.y / wLength);
                float absX = abs(nx) * 2.0; // 0 at center, 1 at edge

                // Flash warning in the final 20% of charge
                float finalFlash = (_Progress > 0.78) ? (step(0.5, frac(_Time.y * 14.0)) * 0.45) : 0.0;
                float pulse = 0.8 + 0.2 * sin(_Time.y * 12.0);

                // =========================================================
                // MODE 1: CIRCLE TELEGRAPH (_ShapeType == 1)
                // =========================================================
                if (_ShapeType > 0.5 && _ShapeType < 1.5)
                {
                    float2 cWorld = float2((IN.uv.x - 0.5) * wWidth, (IN.uv.y - 0.5) * wLength);
                    float2 cPix = floor(cWorld * ppu + 0.5) / ppu;
                    float radius = wWidth * 0.5;
                    float dist = length(cPix);
                    float normR = dist / max(0.1, radius);

                    if (normR > 1.0) discard;

                    float angle = atan2(cPix.y, cPix.x);
                    float deg = degrees(angle) + 180.0;
                    float rotDeg1 = fmod(deg + _Time.y * 65.0, 360.0);
                    float rotDeg2 = fmod(deg - _Time.y * 45.0 + 360.0, 360.0);

                    float pixThickness = 1.5 / (radius * ppu);

                    // Outer solid pixel ring
                    bool outerRing = (normR >= 1.0 - pixThickness * 1.4) && (normR <= 1.0);
                    // Inner rotating dashed rune ring
                    bool innerDashed = (abs(normR - 0.76) <= pixThickness * 0.9) && (fmod(rotDeg1, 22.5) < 13.0);
                    // 8 cardinal rune ticks between inner and outer ring
                    bool runeTick = (normR > 0.76 && normR < 1.0 - pixThickness) && (fmod(rotDeg2, 45.0) < 3.5);
                    // Center crosshair reticle
                    bool centerCross = (normR < 0.22) && (abs(cPix.x) <= (1.0 / ppu) || abs(cPix.y) <= (1.0 / ppu));

                    // Expanding charge ring & fill
                    bool chargeFront = abs(normR - _Progress) <= pixThickness * 1.3;
                    bool insideCharge = normR < _Progress;

                    float alpha = 0.08;
                    fixed3 rgb = _Color.rgb;

                    if (insideCharge)
                    {
                        alpha = lerp(0.12, 0.34, normR / max(0.05, _Progress));
                    }
                    if (innerDashed || runeTick || centerCross)
                    {
                        alpha = max(alpha, 0.68 * pulse);
                        rgb = lerp(_Color.rgb, _CoreColor.rgb, 0.35);
                    }
                    if (outerRing || chargeFront)
                    {
                        alpha = 0.92;
                        rgb = lerp(_Color.rgb, _CoreColor.rgb, chargeFront ? 0.8 : 0.4);
                    }

                    rgb = lerp(rgb, _CoreColor.rgb, finalFlash);
                    return fixed4(rgb, saturate(alpha + finalFlash * 0.25));
                }

                // =========================================================
                // MODE 2: THIN PROJECTILE TRAJECTORY RAY (_ShapeType == 2)
                // =========================================================
                if (_ShapeType >= 1.5)
                {
                    // Taper both ends smoothly
                    float endFade = smoothstep(0.0, 0.06, ny) * (1.0 - smoothstep(0.88, 1.0, ny));
                    if (absX > 0.95 || endFade <= 0.01) discard;

                    // Scrolling pixel laser dashes moving along the bullet path
                    float scrollY = pixWorld.y * 3.5 - _Time.y * 9.0;
                    bool dashOn = frac(scrollY) < 0.62;

                    // Core laser line (2 pixels wide) + soft outer glow
                    bool isCoreLine = abs(pixWorld.x) <= (1.1 / ppu);
                    bool isChargeFilled = ny <= _Progress;

                    float alpha = 0.0;
                    fixed3 rgb = _Color.rgb;

                    if (isCoreLine)
                    {
                        alpha = (dashOn ? 0.88 : 0.35) * endFade;
                        if (isChargeFilled)
                        {
                            alpha = 0.95 * endFade;
                            rgb = lerp(_Color.rgb, _CoreColor.rgb, 0.75);
                        }
                    }
                    else
                    {
                        float glow = (1.0 - absX) * 0.38 * endFade;
                        alpha = isChargeFilled ? glow * 1.35 : glow * 0.75;
                    }

                    // Leading charge spark pulsing down the ray
                    if (abs(ny - _Progress) < 0.035)
                    {
                        alpha = max(alpha, (1.0 - absX * 0.7) * 0.95 * endFade);
                        rgb = _CoreColor.rgb;
                    }

                    rgb = lerp(rgb, _CoreColor.rgb, finalFlash);
                    return fixed4(rgb, saturate(alpha));
                }

                // =========================================================
                // MODE 0: SLEEK PIXEL ENERGY CORRIDOR (Beam / Charge Dash)
                // =========================================================
                // Rounded / tapered start & end caps so it NEVER looks like a boxy rectangle
                float capRadius = min(0.5, (wWidth * 0.45) / max(0.5, wLength));
                float maxAllowedX = 1.0;
                if (ny < capRadius)
                {
                    float t = ny / max(0.001, capRadius);
                    maxAllowedX = sqrt(saturate(t * (2.0 - t)));
                }
                else if (ny > 1.0 - capRadius)
                {
                    float t = (1.0 - ny) / max(0.001, capRadius);
                    maxAllowedX = sqrt(saturate(t * (2.0 - t)));
                }

                if (absX > maxAllowedX) discard;

                float distFromEdge = (maxAllowedX - absX) * (wWidth * 0.5);
                float pixSize = 1.0 / ppu;

                // 1. Sleek 2-pixel side energy rails with scrolling pulse
                bool isSideRail = distFromEdge <= (pixSize * 2.1);
                float railScroll = sin(pixWorld.y * 4.0 - _Time.y * 14.0) * 0.5 + 0.5;

                // 2. Crisp 1-pixel center lock-on dashed line
                bool isCenterLine = (abs(pixWorld.x) <= pixSize * 0.85) && (frac(pixWorld.y * 2.0 - _Time.y * 6.0) < 0.55);

                // 3. Sleek scrolling pixel chevrons (>>>) in uniform world-space pixels
                float chevCoord = (pixWorld.y - abs(pixWorld.x) * 0.85) * 1.4 - _Time.y * 4.5;
                float chevFrac = frac(chevCoord);
                bool isChevron = (absX < maxAllowedX * 0.72) && (chevFrac < 0.14);

                // 4. Charge wave sweeping from Boss (ny=0) to target (ny=_Progress)
                bool isChargeEdge = (abs(ny - _Progress) <= (pixSize * 2.2 / wLength));
                bool isChargedArea = ny <= _Progress;

                // Base corridor fill fades gently toward center so it stays clean and readable
                float edgeGradient = pow(saturate(absX / max(0.01, maxAllowedX)), 1.6) * 0.28;
                float alpha = 0.06 + edgeGradient;
                fixed3 rgb = _Color.rgb;

                if (isChargedArea)
                {
                    alpha += 0.16;
                    rgb = lerp(_Color.rgb, _CoreColor.rgb, 0.22);
                }

                if (isChevron)
                {
                    alpha = max(alpha, isChargedArea ? 0.62 : 0.38);
                    rgb = lerp(_Color.rgb, _CoreColor.rgb, isChargedArea ? 0.55 : 0.25);
                }

                if (isCenterLine)
                {
                    alpha = max(alpha, 0.72);
                    rgb = lerp(_Color.rgb, _CoreColor.rgb, 0.65);
                }

                if (isSideRail)
                {
                    alpha = lerp(0.75, 0.98, railScroll);
                    rgb = lerp(_Color.rgb, _CoreColor.rgb, railScroll * 0.65);
                }

                if (isChargeEdge)
                {
                    alpha = 0.96;
                    rgb = _CoreColor.rgb;
                }

                // Fade the very tip smoothly
                float lengthFade = smoothstep(0.0, 0.03, ny) * (1.0 - smoothstep(0.94, 1.0, ny) * 0.4);
                alpha *= lengthFade;

                rgb = lerp(rgb, _CoreColor.rgb, finalFlash);
                return fixed4(rgb, saturate(alpha + finalFlash * 0.2));
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
