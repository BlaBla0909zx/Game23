Shader "Custom/CoinFloatingShiny"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _FloatSpeed ("Float Speed", Float) = 1.0
        _FloatAmplitude ("Float Amplitude", Float) = 0.5
        _RotationSpeed ("Rotation Speed", Float) = 0.5

        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineSpeed ("Shine Speed", Float) = 1.0
        _ShineWidth ("Shine Width", Float) = 0.2
        _ShineIntensity ("Shine Intensity", Float) = 1.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _FloatSpeed;
                float _FloatAmplitude;
                float _RotationSpeed;

                float4 _ShineColor;
                float _ShineSpeed;
                float _ShineWidth;
                float _ShineIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // Floating animation
                float time = _Time.y;
                float floatOffset = sin(time * _FloatSpeed) * _FloatAmplitude;

                // Rotation
                float rotationAngle = time * _RotationSpeed;
                float cosRot = cos(rotationAngle);
                float sinRot = sin(rotationAngle);

                float3 rotatedPos = input.positionOS.xyz;
                rotatedPos.x = input.positionOS.x * cosRot - input.positionOS.z * sinRot;
                rotatedPos.z = input.positionOS.x * sinRot + input.positionOS.z * cosRot;

                rotatedPos.y += floatOffset;

                output.positionHCS = TransformObjectToHClip(rotatedPos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 baseColor = texColor * _Color;

                // --- Shiny effect ---
                float time = _Time.y * _ShineSpeed;

                // Move shine diagonally across the UV
                float shineMask = saturate(1.0 - abs((input.uv.x + input.uv.y + time) % 1.0 - 0.5) / _ShineWidth);

                half4 shine = _ShineColor * shineMask * _ShineIntensity;

                return baseColor + shine;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
