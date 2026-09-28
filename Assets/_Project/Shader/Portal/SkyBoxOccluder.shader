Shader "Custom/SkyboxOccluder"
{
    Properties
    {
    }

    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

    struct Attributes
    {
        float4 positionOS : POSITION;
    };

    struct Varyings
    {
        float4 positionHCS : SV_POSITION;
        float clipDistance : SV_ClipDistance;
    };
    
    float4 _ClippingPlane;


    Varyings vert(Attributes IN)
    {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        
    	float4 plane = _ClippingPlane;
    	OUT.clipDistance = dot(TransformObjectToWorld(IN.positionOS), plane.xyz) + plane.w;
        return OUT;
    }
    

    float4 frag(Varyings IN) : SV_Target
    {
        return 0;
    }
    
    float fragDepth(Varyings IN) : SV_Depth
    {
        float2 uv = GetNormalizedScreenSpaceUV(IN.positionHCS);
        float depth = SampleSceneDepth(uv);
        
        float ownDepth = IN.positionHCS.z;
        
        if (depth < ownDepth)
        {
            return 0;
        }
        return depth;
    }
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest"}

        Pass
        {
            Tags { "LightMode" = "ClearDepth" }
            
            ColorMask 0
            ZTest LEqual
            ZWrite Off
            
            Stencil
            {
                Ref 32
                WriteMask 32
                Pass Replace
            }
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
