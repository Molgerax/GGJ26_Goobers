Shader "Custom/StencilBlit"
{   
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "StencilBlit"
            
            ColorMask 0
            ZTest Always
            ZWrite On
            Cull Off
            
            Stencil
            {
                Ref 32
                ReadMask 32
                WriteMask 32
                Comp Equal
                Pass Zero
            }
            
            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment frag
            
            
            float frag (Varyings IN) : SV_Depth
            {
                return 0;
            }
            
            ENDHLSL
        }
    }
}
