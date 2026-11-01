Shader "Konoha/AnimeToonOutline"
{
    Properties
    {
        [MainTexture] _BaseMap ("Painted Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ShadowColor ("Cel Shadow", Color) = (0.55,0.43,0.34,1)
        _MidColor ("Cel Midtone", Color) = (0.82,0.78,0.70,1)
        _HighlightColor ("Cel Highlight", Color) = (1,0.98,0.91,1)
        _OutlineColor ("Ink Outline", Color) = (0.14,0.075,0.055,1)
        _OutlineWidth ("Outline Width", Range(0.005,0.08)) = 0.035
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        UsePass "Konoha/AnimeToon/ForwardLit"
        Pass
        {
            Name "InkOutline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings OutlineVert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                positionWS += normalWS * _OutlineWidth;
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }
            half4 OutlineFrag(Varyings input) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
    FallBack "Konoha/AnimeToon"
}
