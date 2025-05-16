Shader "Custom/CanvasToPlaneShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,0.5,0.5,1) // Pink color for visibility
        [Toggle] _DebugMode ("Debug Mode", Float) = 0
        _DebugColor ("Debug Color", Color) = (0,1,0,1) // Green for debug
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100
        
        ZWrite On
        ZTest LEqual
        Cull Back
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 positionHCS : SV_POSITION;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _DebugMode;
                float4 _DebugColor;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Transform position
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                
                // Pass through UVs
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                // Apply fog (URP way)
                output.fogCoord = ComputeFogFactor(output.positionHCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Debug mode - show UV grid pattern to verify mapping is correct
                if (_DebugMode > 0.5) {
                    float2 grid = abs(frac(input.uv * 10) - 0.5);
                    float lineWidth = 0.05;
                    float lines = step(lineWidth, grid.x) * step(lineWidth, grid.y);
                    return lerp(_DebugColor, float4(input.uv, 0, 1), lines);
                }
                
                // Sample texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Check if texture has any actual content
                // In RenderTextures, a completely transparent color often means nothing was drawn
                float contentCheck = texColor.r + texColor.g + texColor.b;
                
                half4 color;
                // If texture is practically empty (all channels near zero), use a pattern based on UVs
                if (contentCheck < 0.01) {
                    // Create a visible checkerboard pattern for debugging
                    float2 checkPos = floor(input.uv * 20);
                    float checker = fmod(checkPos.x + checkPos.y, 2.0);
                    color = lerp(_Color * 0.7, _Color * 1.2, checker);
                }
                else {
                    // Normal rendering when texture has content
                    color = texColor * _Color;
                }
                
                // Apply fog (URP way)
                color.rgb = MixFog(color.rgb, input.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
    }
    
    // Fallback for compatibility
    FallBack "Universal Render Pipeline/Unlit"
}