Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness (pixels)", Range(0, 30)) = 5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "SpriteOutline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _OutlineColor;
                float  _OutlineThickness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                
                if (texColor.a > 0.01)
                    return texColor;
                
                float2 o  = _MainTex_TexelSize.xy * _OutlineThickness;
                float2 o2 = o * 0.5;

                float outline = 0;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x,    0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x,    0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(   0,  o.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(   0, -o.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x,  o.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x,  o.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x, -o.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x, -o.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o2.x,     0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o2.x,     0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(    0,  o2.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(    0, -o2.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o2.x,  o2.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o2.x,  o2.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o2.x, -o2.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o2.x, -o2.y)).a;

                outline = saturate(outline);
                
                return half4(_OutlineColor.rgb, outline);
            }
            ENDHLSL
        }
    }
}