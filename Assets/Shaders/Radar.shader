Shader "Custom/Radar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RadarCenter ("Radar Center", Vector) = (0.5, 0.5, 0, 0)
        _RadarRadius ("Radar Radius", Float) = 0.5
        _SweepSpeed ("Sweep Speed", Float) = 1.0
        _SweepWidth ("Sweep Width (deg)", Float) = 45
        _BackgroundColor ("Background Color", Color) = (0, 0.1, 0, 1)
        _SweepColor ("Sweep Color", Color) = (0, 1, 0, 1)
        _GridColor ("Grid Color", Color) = (0, 0.5, 0, 0.3)
        _GridLines ("Grid Lines", Float) = 4
        _FadeDistance ("Fade Distance", Float) = 0.1
        _LineThickness ("Cross Line Thickness", Float) = 0.002
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _RadarCenter;
            float _RadarRadius;
            float _SweepSpeed;
            float _SweepWidth;
            float4 _BackgroundColor;
            float4 _SweepColor;
            float4 _GridColor;
            float _GridLines;
            float _FadeDistance;
            float _LineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float ring(float2 uv, float2 center, float radius, float thickness)
            {
                float dist = distance(uv, center);
                return smoothstep(radius - thickness, radius, dist) * (1.0 - smoothstep(radius, radius + thickness, dist));
            }

            // normalize angle to [0,360)
            float angleDeg(float2 dir)
            {
                float a = atan2(dir.y, dir.x) * 57.29577951308232; // 180/pi
                if (a < 0) a += 360.0;
                return a;
            }

            // returns signed shortest difference in degrees in range (-180, 180]
            float signedAngleDiff(float a, float b)
            {
                float d = a - b;
                d = fmod(d + 180.0, 360.0) - 180.0;
                return d;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = _BackgroundColor;

                // Distance from radar center
                float dist = distance(uv, _RadarCenter);

                // Only render within radar radius
                if (dist > _RadarRadius)
                {
                    col.a = 0;
                    return col;
                }

                // Grid circles
                for (int j = 1; j <= (int)max(1, _GridLines); j++)
                {
                    float gridRadius = (_RadarRadius / _GridLines) * j;
                    float gridRing = ring(uv, _RadarCenter, gridRadius, 0.005);
                    col.rgb = lerp(col.rgb, _GridColor.rgb, gridRing * _GridColor.a);
                }

                // Grid lines (cross) - correct smoothstep usage
                float2 centered = uv - _RadarCenter;
                float thickness = max(0.0001, _LineThickness);
                float horizontalLine = 1.0 - smoothstep(0.0, thickness, abs(centered.y));
                float verticalLine   = 1.0 - smoothstep(0.0, thickness, abs(centered.x));
                col.rgb = lerp(col.rgb, _GridColor.rgb, (horizontalLine + verticalLine) * 0.5 * _GridColor.a);

                // Radar sweep (robust: use signed angular difference to avoid wrap branches)
                // sweepAngle is the leading edge (we'll treat sweep center for nicer symmetric falloff)
                float sweepLeading = fmod(_Time.y * _SweepSpeed * 60.0, 360.0);
                float halfWidth = max(0.001, _SweepWidth * 0.5);
                float sweepCenter = fmod(sweepLeading + halfWidth, 360.0);

                float2 dir = uv - _RadarCenter;
                float ang = angleDeg(dir); // [0,360)
                float diff = signedAngleDiff(ang, sweepCenter); // (-180,180]
                float absDiff = abs(diff);

                // intensity is 1 at center, falls to 0 at halfWidth
                float sweepIntensity = saturate(1.0 - smoothstep(0.0, halfWidth, absDiff));

                // optional: fade sweep with radius so it doesn't look identical across radius
                float radialFade = 1.0 - smoothstep(0.0, _RadarRadius, dist); // near center stronger
                // combine
                float finalSweep = sweepIntensity * _SweepColor.a * radialFade;
                col.rgb = lerp(col.rgb, _SweepColor.rgb, finalSweep);

                // Fade at edges of radar
                float edgeFade = 1.0 - smoothstep(_RadarRadius - _FadeDistance, _RadarRadius, dist);
                col.a *= edgeFade;

                return col;
            }
            ENDCG
        }
    }
}
