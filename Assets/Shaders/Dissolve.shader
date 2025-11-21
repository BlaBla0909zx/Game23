// Shader "Custom/UnlitDissolve"
// {
    //     Properties
    //     {
        //         _MainTex ("Main Tex", 2D) = "white" {}
        //         _NoiseTex ("Noise (grayscale)", 2D) = "white" {}
        //         _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0.0
        //         _NoiseScale ("Noise Scale", Float) = 1.0
        //         _EdgeWidth ("Edge Width", Range(0.001,0.2)) = 0.02
        //         _UseAlphaClip ("Use Alpha Clip", Float) = 1.0
        //         _Cutoff ("Alpha Cutoff Threshold", Range(0,1)) = 0.01
    //     }

    //     SubShader
    //     {
        //         Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        //         LOD 100

        //         Pass
        //         {
            //             Cull Off
            //             ZWrite On
            //             CGPROGRAM
            //             #pragma vertex vert
            //             #pragma fragment frag
            //             #pragma multi_compile_fog

            //             #include "UnityCG.cginc"

            //             sampler2D _MainTex;
            //             float4 _MainTex_ST;
            //             sampler2D _NoiseTex;
            //             float _DissolveAmount;
            //             float _NoiseScale;
            //             float _EdgeWidth;
            //             float _UseAlphaClip;
            //             float _Cutoff;

            //             struct appdata
            //             {
                //                 float4 vertex : POSITION;
                //                 float2 uv : TEXCOORD0;
            //             };

            //             struct v2f
            //             {
                //                 float2 uv : TEXCOORD0;
                //                 float2 uvNoise : TEXCOORD1;
                //                 float4 vertex : SV_POSITION;
            //             };

            //             v2f vert (appdata v)
            //             {
                //                 v2f o;
                //                 o.vertex = UnityObjectToClipPos(v.vertex);
                //                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //                 // noise uv use world pos jitter potential: here use uv scaled
                //                 o.uvNoise = v.uv * _NoiseScale;
                //                 return o;
            //             }

            //             float4 frag (v2f i) : SV_Target
            //             {
                //                 // sample textures
                //                 float4 col = tex2D(_MainTex, i.uv);
                //                 float noise = tex2D(_NoiseTex, i.uvNoise).r; // assume grayscale

                //                 // compute dissolve: if noise < dissolveAmount -> removed
                //                 // but we want a soft edge: compute a smooth edge mask
                //                 float d = noise;
                //                 float alpha = col.a;

                //                 if (_UseAlphaClip > 0.5)
                //                 {
                    //                     // Hard/soft clip: if noise < dissolve -> discard
                    //                     if (d < _DissolveAmount - _Cutoff)
                    //                     {
                        //                         discard;
                    //                     }
                    //                     // else keep full color (optionally modulate alpha slightly)
                //                 }
                //                 else
                //                 {
                    //                     // Fade out smoothly
                    //                     float reveal = smoothstep(_DissolveAmount - _EdgeWidth, _DissolveAmount + _EdgeWidth, d);
                    //                     alpha *= reveal;
                    //                     if (alpha <= 0.001) discard;
                //                 }

                //                 // edge color add (emission-like)
                //                 float3 finalRGB = col.rgb;

                //                 return float4(finalRGB, alpha);
            //             }
            //             ENDCG
        //         }
    //     }

    //     FallBack "Diffuse"
// }

Shader "Universal Render Pipeline/Custom/LitDissolve"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _NoiseTex ("Noise (grayscale)", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0.0
        _NoiseScale ("Noise Scale", Float) = 1.0
        _EdgeWidth ("Edge Width", Range(0.001,0.2)) = 0.02
        _EdgeColor ("Edge Color", Color) = (1,0.5,0,1)
        _EdgeIntensity ("Edge Intensity", Float) = 2.0
        _UseAlphaClip ("Use Alpha Clip", Float) = 1.0
        _Cutoff ("Alpha Cutoff Threshold", Range(0,1)) = 0.5
        
        // Standard lit properties
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _OcclusionMap ("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1.0
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "Queue" = "AlphaTest" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Off
            ZWrite On
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex vert
            #pragma fragment frag
            
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            
            // Unity keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ _ALPHATEST_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseTex);           SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _EdgeColor;
                float _DissolveAmount;
                float _NoiseScale;
                float _EdgeWidth;
                float _EdgeIntensity;
                float _UseAlphaClip;
                float _Cutoff;
                float _Metallic;
                float _Smoothness;
                float _BumpScale;
                float _OcclusionStrength;
                float4 _EmissionColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                float2 lightmapUV   : TEXCOORD1;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float2 uvNoise      : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
                float3 normalWS     : TEXCOORD3;
                float4 tangentWS    : TEXCOORD4;
                float3 viewDirWS    : TEXCOORD5;
                float fogCoord      : TEXCOORD6;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 7);
                float4 positionCS   : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.uvNoise = input.texcoord * _NoiseScale;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w);
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample textures
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 baseColor = albedoAlpha * _BaseColor;
                
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.uvNoise).r;
                
                // Dissolve calculation
                half alpha = baseColor.a;
                half edgeFactor = 0.0;
                
                if (_UseAlphaClip > 0.5)
                {
                    // Hard clip with edge glow
                    half dissolveEdge = _DissolveAmount + _EdgeWidth;
                    if (noise < _DissolveAmount - _Cutoff)
                    {
                        discard;
                    }
                    
                    // Calculate edge glow
                    if (noise < dissolveEdge)
                    {
                        edgeFactor = smoothstep(_DissolveAmount - _EdgeWidth, _DissolveAmount, noise);
                    }
                }
                else
                {
                    // Smooth fade
                    half reveal = smoothstep(_DissolveAmount - _EdgeWidth, _DissolveAmount + _EdgeWidth, noise);
                    alpha *= reveal;
                    if (alpha <= 0.001) discard;
                    
                    edgeFactor = 1.0 - reveal;
                }
                
                // Sample additional maps
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                half occlusion = lerp(1.0, SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, input.uv).g, _OcclusionStrength);
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                
                // Calculate world space normal
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w, input.normalWS));
                normalWS = normalize(normalWS);
                
                // Setup surface data
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = SafeNormalize(input.viewDirWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogCoord;
                inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0.0, 0.0, 0.0);
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = emission + (_EdgeColor.rgb * edgeFactor * _EdgeIntensity);
                surfaceData.occlusion = occlusion;
                surfaceData.alpha = alpha;
                
                // Calculate lighting
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // Apply fog
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull Off
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}