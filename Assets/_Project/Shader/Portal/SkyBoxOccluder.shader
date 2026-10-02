Shader "Custom/SkyboxOccluder"
{
    Properties
    {
        _MainTex("Texture Preview", 2D) = "black" {}
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
        float4 clipDist0 : SV_ClipDistance0;
        float2 clipDist1 : SV_ClipDistance1;
    };
    
    
    float4 _ClippingPlanes[6];
    
    
    Varyings vert(Attributes IN)
    {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        
    	float4 pos = float4(TransformObjectToWorld(IN.positionOS), 1);

    	OUT.clipDist0 = 1000;
    	OUT.clipDist1 = 1000;
    	
    	OUT.clipDist0[0] = dot(_ClippingPlanes[0], pos);
		OUT.clipDist0[1] = dot(_ClippingPlanes[1], pos);
		OUT.clipDist0[2] = dot(_ClippingPlanes[2], pos);
		OUT.clipDist0[3] = dot(_ClippingPlanes[3], pos);
		OUT.clipDist1[0] = dot(_ClippingPlanes[4], pos) + 0.001;
		OUT.clipDist1[1] = dot(_ClippingPlanes[5], pos);
        
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
