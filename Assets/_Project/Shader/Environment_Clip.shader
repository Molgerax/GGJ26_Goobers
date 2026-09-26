Shader "Custom/Environment_Clip" 
{
    Properties
    {
		[MainTex] _MainTex("Main Texture", 2D) = "white" {}
		[MainColor] _Color("Color", Color) = (1,1,1,1)
		[Normal] _BumpMap("Normal", 2D) = "bump" {} // Bump is the default texture for normal maps
    	
    	_Shininess("Shiny", Float) = 8
    }
    
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    
    #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
    #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
    
    #pragma target 3.5
    #pragma shader_feature _ _SHADOWMODE_ON
	#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
    #pragma multi_compile_fwdadd_fullshadows
    
    struct Attributes
    {
    	float4 pos : POSITION;
    	float4 uv : TEXCOORD0;
    	float4 uv2 : TEXCOORD1;
    	float3 normal : NORMAL;
    	float4 tangent : TANGENT;
    };
    
    struct Varyings
    {
    	float4 positionHCS : SV_POSITION;
    	float2 uv : TEXCOORD0;
    	float3 positionWS : TEXCOORD1;
    	float4 tangentWS : TEXCOORD2;
    	float3 normalWS : TEXCOORD3;
    	float clipDistance	: SV_ClipDistance;
    	
		DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
    };
    
    
    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    TEXTURE2D(_BumpMap);
    SAMPLER(sampler_BumpMap);
	
    float4 _ClippingPlane;
    
    CBUFFER_START(UnityPerMaterial)
		float4 _MainTex_ST;
		float4 _Color;
    	float4 _BumpMap_ST;
    	float _Shininess;
    CBUFFER_END
    
    // Shader Functions-----------------------
    Varyings vert(Attributes IN)
    {
    	Varyings OUT;
    	

		// New way of doing it
    	VertexNormalInputs inputs = GetVertexNormalInputs(IN.normal, IN.tangent);
    	OUT.tangentWS = float4(inputs.tangentWS, IN.tangent.w);
    	OUT.normalWS = inputs.normalWS;

    	
    	OUT.positionHCS = TransformObjectToHClip(IN.pos);
    	OUT.positionWS = TransformObjectToWorld(IN.pos);
    	OUT.uv = IN.uv;
    	
    	
    	float4 plane = _ClippingPlane;
    	plane *= -1;
    	
    	OUT.clipDistance = dot(OUT.positionWS, -plane.xyz) - plane.w;
    	
		OUTPUT_LIGHTMAP_UV( IN.uv2, unity_LightmapST, OUT.lightmapUV);
		OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);
    	
    	return OUT;
    }
    
    float4 frag(Varyings IN) : COLOR 
    {	
    	// First we sample the normal texture
    	float4 encodedNormal = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, TRANSFORM_TEX(IN.uv, _BumpMap));

    	// Then we decode it from the texture, Unity takes care of the way the textures are packed here
    	float3 decodedNormal = UnpackNormal(encodedNormal);
    	// Then we construct the TangentToWorld transformation matrix at this specific pixel
    	float3x3 tangentToWorld = CreateTangentToWorld(IN.normalWS, IN.tangentWS.xyz, IN.tangentWS.w);
    	// At last, we transform the decoded normal from tangent space to world space, so we can use it as usual
    	float3 normalWS = TransformTangentToWorldDir(decodedNormal, tangentToWorld, true);
    	
    	float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex)) * _Color.rgb;

    	InputData lightingInput = (InputData)0; // Found in URP/ShaderLib/Input.hlsl
		lightingInput.positionWS = IN.positionWS;
		lightingInput.normalWS = normalize(IN.normalWS);
		lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS); // In ShaderVariablesFunctions.hlsl
		lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS); // In Shadows.hlsl
		lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
    	lightingInput.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, IN.normalWS);
		SurfaceData surfaceInput = (SurfaceData)0;
		surfaceInput.albedo = color;
		surfaceInput.alpha = 1;
		surfaceInput.specular = 1;
		surfaceInput.smoothness = 0;
		surfaceInput.metallic = 0;
		surfaceInput.occlusion = 1;
	
		#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
		float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
		AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
		//surfaceInput.occlusion *= aoFactor.directAmbientOcclusion;
		#endif

	
		return UniversalFragmentPBR(lightingInput, surfaceInput);
    }
    
    ENDHLSL

    
    SubShader
    {
    	Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry" 
        }

        Pass //Base with Ambient Light
    	{
    		Name "ForwardLit"
	        Tags { "LightMode" = "UniversalForward" "RenderType"="Opaque"}
	
	        ZWrite On
    		Cull Back
	
        	
        	HLSLPROGRAM

        	#define _SPECULAR_COLOR
        	
        	#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

        	#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
        	#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS

        	
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
    		#pragma vertex vert
			#pragma fragment frag
        	
        	
        	ENDHLSL
		}
		Pass
        {
			Tags {"LightMode" = "DepthOnly"}

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment fragDepth
            
            half fragDepth(Varyings input) : SV_TARGET
			{
				return input.positionHCS.z;
			}
            
			ENDHLSL
        }
    }
}