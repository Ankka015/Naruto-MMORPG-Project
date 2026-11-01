Shader "Konoha/AnimeToon"
{
    Properties
    {
        [MainTexture] _BaseMap ("Painted Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ShadowColor ("Cel Shadow", Color) = (0.55,0.43,0.34,1)
        _MidColor ("Cel Midtone", Color) = (0.82,0.78,0.70,1)
        _HighlightColor ("Cel Highlight", Color) = (1,0.98,0.91,1)
        _WindStrength ("Wind Motion", Range(0,0.35)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ToonVert
            #pragma fragment ToonFrag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _MidColor;
                half4 _HighlightColor;
                float _WindStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };
            Varyings ToonVert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                float3 positionWS = positionInputs.positionWS;
                float windPhase = _Time.y * 1.55 + positionWS.x * 0.075 + positionWS.z * 0.055;
                positionWS.x += sin(windPhase) * _WindStrength;
                positionWS.z += sin(windPhase * 0.73 + 1.2) * _WindStrength * 0.42;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = TransformWorldToShadowCoord(positionWS);
                return output;
            }
            half4 ToonFrag(Varyings input) : SV_Target
            {
                half4 painted = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight(input.shadowCoord);
                half diffuse = saturate(dot(normalWS, mainLight.direction)) * mainLight.shadowAttenuation;
                half3 cel = _ShadowColor.rgb;
                cel = lerp(cel, _MidColor.rgb, step(0.20h, diffuse));
                cel = lerp(cel, _HighlightColor.rgb, step(0.68h, diffuse));
                half3 ambient = SampleSH(normalWS);
                half3 color = painted.rgb * (cel * mainLight.color + ambient * 0.28h);
                return half4(color, painted.a);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
    FallBack "Universal Render Pipeline/Lit"
}
