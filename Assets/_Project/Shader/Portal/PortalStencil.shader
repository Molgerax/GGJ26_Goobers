Shader "Custom/PortalStencil"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }

    HLSLINCLUDE
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct Attributes
    {
        float4 positionOS : POSITION;
    };

    struct Varyings
    {
        float4 positionHCS : SV_POSITION;
    };

    CBUFFER_START(UnityPerMaterial)
        half4 _BaseColor;
    CBUFFER_END

    Varyings vert(Attributes IN)
    {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        return OUT;
    }

    half4 frag(Varyings IN) : SV_Target
    {
        return _BaseColor;
    }
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ColorMask 0
            ZTest LEqual
            ZWrite Off
            Cull Off
            
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
            
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            ColorMask 0
            ZTest Always
            ZWrite On
            Cull Off
            
            Stencil
            {
                Ref 1
                Comp Equal
                Pass DecrSat
                ZFail DecrSat
            }
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
