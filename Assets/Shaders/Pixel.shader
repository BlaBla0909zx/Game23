Shader "Hidden/Pixel"
{
    Properties
    {
        // We keep _MainTex property for compatibility in the editor, but the blitter
        // will actually provide the source image in _BlitTexture at runtime.
        _MainTex ("Texture", 2D) = "white" {}
        _CameraDepthTexture ("Depth Texture", 2D) = "white" {}
        _CameraDepthNormalsTexture ("Depth Normals Texture", 2D) = "white" {}
        _LightIntensity ("Light Intensity", Float) = 1.25
        _LineAlpha ("Line Alpha", Range(0,1)) = 0.7
        _UseLighting ("Use Lighting", Float) = 1.0
        _LineHighlight ("Line Highlight", Range(0,1)) = 0.2
        _LineShadow ("Line Shadow", Range(0,1)) = 0.55
    }

    SubShader
    {
        // Post-process style: don't write depth, always draw
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv         : TEXCOORD0;
                float4 vertex     : SV_POSITION;
                float2 screenUV   : TEXCOORD1;
            };

            // NOTE: At runtime URP Blitter sets the current camera color as _BlitTexture.
            sampler2D _BlitTexture;                            // source (camera) color provided by Blitter
            sampler2D _MainTex;                               // kept for inspector compatibility
            sampler2D _CameraDepthTexture;                    // depth (SAMPLE_DEPTH_TEXTURE)
            sampler2D _CameraDepthNormalsTexture;             // encoded depth+normal texture from URP
            float4 _MainTex_TexelSize;                        // texel size (if needed)

            // parameters
            float _LightIntensity;
            float _LineAlpha;
            float _UseLighting;
            float _LineHighlight;
            float _LineShadow;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenUV = v.uv;
                return o;
            }

            // Return linear eye depth from depth texture sample
            float GetLinearDepth(float2 sUV, float mask)
            {
                // SAMPLE_DEPTH_TEXTURE works with Unity depth texture macros
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sUV) * mask;
                // LinearEyeDepth expects depth in [0,1] as returned by SAMPLE_DEPTH_TEXTURE
                return LinearEyeDepth(rawDepth);
            }

            // Decode encoded depth+normal RT produced by URP
            float3 GetNormal(float2 uv, float mask)
            {
                float4 depthNormal = tex2D(_CameraDepthNormalsTexture, uv);
                float3 normal;
                float depth;
                // DecodeDepthNormal is available in UnityCG for URP-style depthNormals encoding
                DecodeDepthNormal(depthNormal, depth, normal);
                return normal * mask;
            }

            float NormalEdgeIndicator(float3 normalEdgeBias, float3 normal, float3 neighborNormal, float depthDifference)
            {
                float normalDifference = dot(normal - neighborNormal, normalEdgeBias);
                float normalIndicator = saturate(smoothstep(-0.01, 0.01, normalDifference));
                float depthIndicator = saturate(sign(depthDifference * 0.25 + 0.0025));
                return (1.0 - dot(normal, neighborNormal)) * depthIndicator * normalIndicator;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // // Use _MainTex_TexelSize if provided by material; otherwise fall back to 1.0/viewport via shader params.
                // float2 texelSize = _MainTex_TexelSize.xy;
                // float2 screenUV = i.screenUV;

                // // 4-neighbor offsets (up, down, right, left)
                // float2 UVOffsets[4];
                // UVOffsets[0] = screenUV + float2(0.0, -1.0) * texelSize;
                // UVOffsets[1] = screenUV + float2(0.0,  1.0) * texelSize;
                // UVOffsets[2] = screenUV + float2(1.0,  0.0) * texelSize;
                // UVOffsets[3] = screenUV + float2(-1.0, 0.0) * texelSize;

                // // We don't rely on depthNormals alpha for masking here.
                // float outlineMask = 1.0;

                // // Depth edge detection
                // float depthDifference = 0.0;
                // float invDepthDifference = 0.5;
                // float depth = GetLinearDepth(screenUV, outlineMask);

                // for (int j = 0; j < 4; j++)
                // {
                    //     float dOff = GetLinearDepth(UVOffsets[j], outlineMask);
                    //     // use absolute difference for stability across depth encoding conventions
                    //     depthDifference += saturate(abs(dOff - depth));
                    //     invDepthDifference += abs(dOff - depth);
                // }

                // invDepthDifference = saturate(invDepthDifference);
                // invDepthDifference = saturate(smoothstep(0.9, 0.9, invDepthDifference) * 10.0);
                // // smoothstep to threshold depthDifference
                // depthDifference = smoothstep(0.25, 0.3, depthDifference);

                // // Normal edge detection
                // float normalDifference = 0.0;
                // float3 normalEdgeBias = float3(1.0, 1.0, 1.0);
                // float3 normal = GetNormal(screenUV, outlineMask);

                // for (int k = 0; k < 4; k++)
                // {
                    //     float3 nOff = GetNormal(UVOffsets[k], outlineMask);
                    //     normalDifference += NormalEdgeIndicator(normalEdgeBias, normal, nOff, depthDifference);
                // }

                // normalDifference = smoothstep(0.2, 0.2, normalDifference);
                // normalDifference = saturate(normalDifference - invDepthDifference);

                // // Sample source color from the blitter-provided texture
                // float3 col = tex2D(_BlitTexture, screenUV).rgb;

                // // Combine edge signals and clamp BEFORE lerp to avoid extrapolation
                // float edgeFactor = saturate(depthDifference + normalDifference * 5.0);
                // float lineMask = saturate(lerp(0.1, _LineAlpha, edgeFactor));

                // // If lighting is off, apply simple cel-ish highlight/shadow for edge
                // if (_UseLighting < 0.5)
                // {
                    //     col += saturate((normalDifference - depthDifference)) * _LineHighlight;
                    //     col -= col * depthDifference * _LineShadow;
                // }
                // else
                // {
                    //     // If you want to modulate lighting when using actual lighting,
                    //     // you can expose DIFFUSE_LIGHT from C# or compute simple shading here.
                    //     // For now, we leave original color unchanged when useLighting == true,
                    //     // because lighting is handled by URP earlier in the pipeline.
                // }

                // return fixed4(col, 1.0);
                return fixed4(1.0, 0.0, 0.0, 1.0); // debug red
            }
            ENDCG
        }
    }
}
