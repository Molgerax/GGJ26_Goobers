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
    

    float frag(Varyings IN) : SV_Depth
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
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry"}

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            ColorMask 0
            ZTest Always
            ZWrite On
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
