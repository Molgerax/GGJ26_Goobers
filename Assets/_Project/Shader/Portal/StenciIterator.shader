Shader "Stencil/StencilIterator"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Int) = 0
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite", Int) = 0
        [Enum(Off, 0, On, 1)]_ZClip("ZClip", Int) = 0
        
        [Space(5)]
        _OffsetFactor("Offset Factor", Float) = 0
        _OffsetUnits("Offset Units", Int) = 0
        
        [Enum(False, 0, True, 1)]_Conservative("Conservative", Int) = 0
        
        [Header(Stencil)]
        [Space(5)]
        _ReadMask("ReadMask", Int) = 255
        _WriteMask("WriteMask", Int) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)]_Comp("Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_Pass("Pass", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_Fail("Fail", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_ZFail("ZFail", Int) = 0
        
        [Space(5)]
        [Enum(UnityEngine.Rendering.CompareFunction)]_CompBack("Comparison Back", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_PassBack("Pass Back", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_FailBack("Fail Back", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_ZFailBack("ZFail Back", Int) = 0
        
        [Space(5)]
        [Enum(UnityEngine.Rendering.CompareFunction)]_CompFront("Comparison Front", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_PassFront("Pass Front", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_FailFront("Fail Front", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_ZFailFront("ZFail Front", Int) = 0
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


    Varyings vert(Attributes IN)
    {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        return OUT;
    }

    half4 frag(Varyings IN) : SV_Target
    {
        return 0;
    }
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass // 0 
        {
            Stencil
            {
                Ref 0
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass // 1 
        {
            Stencil
            {
                Ref 1
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass // 2
        {
            Stencil
            {
                Ref 2
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
                
        Pass // 3
        {
            Stencil
            {
                Ref 3
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass // 4
        {
            Stencil
            {
                Ref 4
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass // 5
        {
            Stencil
            {
                Ref 5
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass // 6
        {
            Stencil
            {
                Ref 6
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass // 7
        {
            Stencil
            {
                Ref 7
                Comp [_Comp]
                Pass [_Pass]
                Fail [_Fail]
                ZFail [_ZFail]
                
                CompBack [_CompBack]
                PassBack [_PassBack]
                FailBack [_FailBack]
                ZFailBack [_ZFailBack]
                
                CompFront [_CompFront]
                PassFront [_PassFront]
                FailFront [_FailFront]
                ZFailFront [_ZFailFront]
            }
            
            ColorMask 0
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            Cull [_Cull]
            Offset [_OffsetFactor], [_OffsetUnits]
            Conservative [_Conservative]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
