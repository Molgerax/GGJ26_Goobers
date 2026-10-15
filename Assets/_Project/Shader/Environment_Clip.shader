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
	#pragma multi_compile_fwdbase 
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
    	//float clipDistance	: SV_ClipDistance;
    	float4 clipDist0	: SV_ClipDistance0;
    	float4 clipDist1	: SV_ClipDistance1;
    	
		DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
    	float fogFactor : TEXCOORD5;
    };
    
    
    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    TEXTURE2D(_BumpMap);
    SAMPLER(sampler_BumpMap);
    
    float4 _ClippingPlanes[6];

    
    CBUFFER_START(UnityPerMaterial)
		float4 _MainTex_ST;
		float4 _Color;
    	float4 _BumpMap_ST;
    	float _Shininess;
    CBUFFER_END
    
    
    Texture2D<uint> _MoldTex;
    
    float3 _MoldCenter;
    float3 _MoldSize;
    uint _MoldResolution;
    
    float3 WorldPosToMoldUV(float3 worldPos)
    {
	    float3 uv = worldPos;
    	
    	uv -= _MoldCenter;
    	uv /= max(0.001, _MoldSize);
    	uv += 0.5;
    	
    	return uv;
    }
    
    uint3 WorldPosToMoldIndex(float3 worldPos)
    {
	    float3 uv = WorldPosToMoldUV(worldPos);
    	uint3 index = float3(uv.xz * _MoldResolution, 0);
    	return index;
    }
    
    
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

    	float4 pos = float4(OUT.positionWS, 1);

    	OUT.clipDist0 = 1000;
    	OUT.clipDist1 = 1000;
    	
    	OUT.clipDist0[0] = dot(_ClippingPlanes[0], pos);
		OUT.clipDist0[1] = dot(_ClippingPlanes[1], pos);
		OUT.clipDist0[2] = dot(_ClippingPlanes[2], pos);
		OUT.clipDist0[3] = dot(_ClippingPlanes[3], pos);
		OUT.clipDist1[0] = dot(_ClippingPlanes[4], pos) + 0.001;
		OUT.clipDist1[1] = dot(_ClippingPlanes[5], pos);
    	
    	//OUT.clipDistance = dist;

    	VertexPositionInputs vpi = GetVertexPositionInputs(IN.pos);
    	OUT.fogFactor = ComputeFogFactor(vpi.positionCS.z);
    	
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
    	
    	
    	uint3 moldIndex = WorldPosToMoldIndex(IN.positionWS);
    	uint moldSample = _MoldTex.Load(moldIndex);
    	float3 moldUV = WorldPosToMoldUV(IN.positionWS);
    	
    	//if (moldUV.x < 0 || moldUV.x > 1 || moldUV.y < 0 || moldUV.y > 1 || moldUV.z < 0 || moldUV.z > 1)
    	//	moldSample = 0;
    	
    	float moldInfluence = saturate(1 - moldUV.y);
    	
    	
    	float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex)).rgb * _Color.rgb;
    	
    	if ((moldSample & 1) > 0)
    		color.gb += 0.5 * moldInfluence;

    	InputData lightingInput = (InputData)0; // Found in URP/ShaderLib/Input.hlsl
		lightingInput.positionWS = IN.positionWS;
		lightingInput.normalWS = normalize(IN.normalWS);
		lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS); // In ShaderVariablesFunctions.hlsl
		lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS); // In Shadows.hlsl
		lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
    	lightingInput.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, IN.normalWS);
    	lightingInput.shadowMask = 0;
    	
    	lightingInput.fogCoord = InitializeInputDataFog(float4(IN.positionWS, 1), IN.fogFactor);
    	
		SurfaceData surfaceInput = (SurfaceData)0;
		surfaceInput.albedo = color;
		surfaceInput.alpha = 1;
		surfaceInput.specular = 1;
		surfaceInput.smoothness = moldSample.x;
		surfaceInput.metallic = 0;
		surfaceInput.occlusion = 1;
	
		#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
		float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
		AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
		//surfaceInput.occlusion *= aoFactor.directAmbientOcclusion;
		#endif

    	float4 finalColor = UniversalFragmentPBR(lightingInput, surfaceInput);
    
    	finalColor.rgb = MixFog(finalColor.rgb, lightingInput.fogCoord);
		return finalColor;
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
        	#pragma multi_compile_fog

        	
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

		Pass
        {
			Tags {"LightMode" = "ShadowCaster"}

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment fragShadow
            
            half fragShadow(Varyings input) : SV_TARGET
			{
				return 0;
			}
            
			ENDHLSL
        }
    }
}