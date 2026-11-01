Shader "Konoha/FlowingWater"
{
    Properties
    {
        [MainTexture] _BaseMap ("Painted Water Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Water Color", Color) = (0.16,0.50,0.59,1)
        _FlowSpeed ("Flow Speed", Range(0,0.5)) = 0.11
        _WaveStrength ("Wave Highlights", Range(0,1)) = 0.38
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "FlowingSurface"
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
                float _FlowSpeed;
                float _WaveStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 flowUV = input.uv;
                flowUV.x += _Time.y * _FlowSpeed;
                half4 painted = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, flowUV) * _BaseColor;
                float bands = sin(input.uv.y * 79.0 + sin(flowUV.x * 4.0) * 2.2 + _Time.y * 1.5);
                float spark = sin(flowUV.x * 47.0 + input.uv.y * 11.0 - _Time.y * 2.4);
                float glint = smoothstep(0.62,0.96,bands * 0.68 + spark * 0.32);
                float slow = 0.91 + 0.09 * sin(flowUV.x * 8.0 + input.uv.y * 13.0 - _Time.y);
                painted.rgb *= slow;
                painted.rgb = lerp(painted.rgb, painted.rgb + half3(0.28,0.48,0.43), glint * _WaveStrength);
                return painted;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
