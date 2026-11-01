Shader "Konoha/GrassWind"
{
    Properties
    {
        [MainTexture] _BaseMap ("Grass Brush Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Grass Tint", Color) = (1,1,1,1)
        _WindStrength ("Wind Motion", Range(0,0.35)) = 0.18
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "GrassLeaves"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _WindStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float bladeHeight = saturate(input.positionOS.y * 1.7);
                float phase = _Time.y * 1.8 + positionWS.x * 0.16 + positionWS.z * 0.13;
                positionWS.x += sin(phase) * _WindStrength * bladeHeight;
                positionWS.z += sin(phase * 0.73 + 1.1) * _WindStrength * 0.45 * bladeHeight;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = input.color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 painted = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor * input.color;
                return painted;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
