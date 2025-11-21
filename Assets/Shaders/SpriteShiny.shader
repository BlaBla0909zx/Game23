Shader "Custom/SpriteShiny"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        [Toggle] _ShinyEnabled ("Enable Shiny", Float) = 1
        _ShinyProgress ("Shiny Progress", Range(0, 1)) = 0.0
        _ShinyWidth ("Shiny Width", Range(0.01, 0.5)) = 0.2
        _ShinyIntensity ("Shiny Intensity", Range(0, 2)) = 1
        _ShinyDir ("Shiny Direction", Vector) = (1, 0, 0, 0)
        _DarkenFactor ("Darken Before Shiny", Range(0,1)) = 0.7
        _ShinyColor ("Shiny Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _ShinyWidth, _ShinyIntensity, _ShinyEnabled;
            float4 _ShinyDir;
            float _DarkenFactor;
            float4 _ShinyColor;
            float _ShinyProgress;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                if (_ShinyEnabled > 0.5)
                {
                    float2 dir = normalize(_ShinyDir.xy);
                    dir = length(dir) < 0.001 ? float2(1, 0) : dir;
                    float shinyPos = saturate(_ShinyProgress);
                    float proj = dot(i.uv, dir);

                    // Darken sau shiny
                    if (proj > shinyPos)
                    {
                        col.rgb *= _DarkenFactor;
                    }

                    // Mask shiny
                    float dist = abs(proj - shinyPos);
                    float shinyMask = saturate(1.0 - dist / _ShinyWidth);
                    shinyMask = pow(shinyMask, 2.0);
                    shinyMask = smoothstep(0.0, 1.0, shinyMask);
                    col.rgb = lerp(col.rgb, _ShinyColor.rgb, shinyMask * _ShinyIntensity);
                    col.rgb += shinyMask * _ShinyIntensity;
                }

                return col;
            }

            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
