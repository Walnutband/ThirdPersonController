Shader "Custom/URP_FadeOut"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MinDistance ("Minimum Distance", float) = 2
        _MaxDistance ("Maximum Distance", float) = 3
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        // 第一个Pass：深度写入关闭，颜色屏蔽
        Pass
        {
            Name "DepthMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            ZWrite Off
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        
        // 第二个Pass：主要的透明渲染
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _MinDistance;
                float _MaxDistance;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 将顶点位置从物体空间转换到裁剪空间
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                // 计算世界空间位置
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                // 处理UV变换
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // 采样纹理
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * _Color;
                
                // 计算到相机的距离
                float3 cameraPosWS = GetCameraPositionWS();
                float distanceFromCamera = distance(input.positionWS, cameraPosWS);
                
                // 计算淡出效果
                float fade = saturate((distanceFromCamera - _MinDistance) / _MaxDistance);
                
                // 应用透明度
                finalColor.a *= fade;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}