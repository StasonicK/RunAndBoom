// Toony Colors Pro+Mobile 2
// (c) 2014-2019 Jean Moreno

Shader "Toony Colors Pro 2/Examples LWRP/Cat Demo/Vertex Colors Specular"
{
	Properties
	{
		[TCP2HeaderHelp(Base)]
		_BaseColor ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		_MainTex ("Albedo", 2D) = "white" {}
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		
		_RampThreshold ("Threshold", Range(0.01,1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 1
		[TCP2Separator]
		
		[TCP2HeaderHelp(Specular)]
		[TCP2ColorNoAlpha] _SpecularColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_SpecularSmoothnessPBR ("Smoothness", Range(0,1)) = 0.5
		[TCP2Separator]

		[TCP2HeaderHelp(Emission)]
		[TCP2ColorNoAlpha] [HDR] _Emission ("Emission Color", Color) = (0,0,0,1)
		[TCP2Separator]
		
		[TCP2HeaderHelp(Rim Lighting)]
		[TCP2ColorNoAlpha] _RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.5)
		_RimMin ("Rim Min", Range(0,2)) = 0.5
		_RimMax ("Rim Max", Range(0,2)) = 1
		[TCP2Separator]
		
		[ToggleOff(_RECEIVE_SHADOWS_OFF)] _ReceiveShadowsOff ("Receive Shadows", Float) = 1

		//Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "LightweightPipeline"
			"RenderType"="Opaque"
		}

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
		ENDHLSL
		Pass
		{
			Name "Main"
			Tags { "LightMode"="LightweightForward" }

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 3.0

			#define fixed half
			#define fixed2 half2
			#define fixed3 half3
			#define fixed4 half4

			// -------------------------------------
			// Material keywords
			//#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Lightweight Render Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

			// -------------------------------------

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex Vertex
			#pragma fragment Fragment

			// Uniforms
			CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _BaseColor;
			half3 _Emission;
			float _RampThreshold;
			float _RampSmoothing;
			fixed3 _SColor;
			fixed3 _HColor;
			float _RimMin;
			float _RimMax;
			fixed3 _RimColor;
			float _SpecularSmoothnessPBR;
			fixed3 _SpecularColor;
			
			//Specular help functions (from UnityStandardBRDF.cginc)
			inline half3 SafeNormalize(half3 inVec)
			{
				half dp3 = max(0.001f, dot(inVec, inVec));
				return inVec * rsqrt(dp3);
			}
			
			//PBR Blinn-Phong
			#define TCP2_PI 3.14159265359
			inline half PercRoughnessToSpecPower(half roughness)
			{
				half sq = max(1e-4f, roughness*roughness);
				half n = (2.0 / sq) - 2.0;
				n = max(n, 1e-4f);
				return n;
			}
			inline half NDFBlinnPhong(half NdotH, half n)
			{
				// norm = (n+2)/(2*pi)
				half normTerm = (n + 2.0) * (0.5/TCP2_PI);
			
				half specTerm = pow (NdotH, n);
				return specTerm * normTerm;
			}
			CBUFFER_END

			// vertex input
			struct Attributes
			{
				float4 vertex       : POSITION;
				float3 normal       : NORMAL;
				float4 tangent      : TANGENT;
				half4 vertexColor   : COLOR;
				float4 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// vertex output / fragment input
			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float3 normal         : NORMAL;
				float4 worldPosAndFog : TEXCOORD0;
			#ifdef _MAIN_LIGHT_SHADOWS
				float4 shadowCoord    : TEXCOORD1; // compute shadow coord per-vertex for the main light
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				half3 vertexLights : TEXCOORD2;
			#endif
				float4 vertexColor : TEXCOORD3;
				float2 pack1 : TEXCOORD4; /* pack1.xy = texcoord0 */
			};

			Varyings Vertex(Attributes input)
			{
				Varyings output;

				// Texture Coordinates
				output.pack1.xy.xy = (input.texcoord0.xy) * _MainTex_ST.xy + _MainTex_ST.zw;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
			#ifdef _MAIN_LIGHT_SHADOWS
				output.shadowCoord = GetShadowCoord(vertexInput);
			#endif

				VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normal);
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				// Vertex lighting
				output.vertexLights = VertexLighting(vertexInput.positionWS, vertexNormalInput.normalWS);
			#endif

				// world position
				output.worldPosAndFog = float4(vertexInput.positionWS.xyz, 0);

				// normal
				output.normal = NormalizeNormalPerVertex(vertexNormalInput.normalWS);

				// clip position
				output.positionCS = vertexInput.positionCS;

				output.vertexColor = input.vertexColor;

				return output;
			}

			half4 Fragment(Varyings input) : SV_Target
			{
				float3 positionWS = input.worldPosAndFog.xyz;
				float3 normalWS = NormalizeNormalPerPixel(input.normal);
				half3 viewDirWS = SafeNormalize(GetCameraPositionWS() - positionWS);

				// Shader Properties Sampling
				float4 __albedo = ( tex2D(_MainTex, (input.pack1.xy.xy)).rgba * input.vertexColor.rgba );
				float4 __mainColor = ( _BaseColor.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float3 __emission = ( _Emission.rgb * input.vertexColor.aaa );
				float __rampThreshold = ( _RampThreshold );
				float __rampSmoothing = ( _RampSmoothing );
				float3 __shadowColor = ( _SColor.rgb );
				float3 __highlightColor = ( _HColor.rgb );
				float __rimMin = ( _RimMin );
				float __rimMax = ( _RimMax );
				float3 __rimColor = ( _RimColor.rgb );
				float __rimStrength = ( 1.0 );
				float __specularSmoothnessPbr = (  1 - __albedo.a * _SpecularSmoothnessPBR );
				float3 __specularColor = ( _SpecularColor.rgb );
				float __ambientIntensity = ( 1.0 );

				half ndv = max(0, dot(viewDirWS, normalWS));
				half ndvRaw = ndv;

				// main texture
				half3 albedo = __albedo.rgb;
				half alpha = __alpha;
				half3 emission = half3(0,0,0);

				albedo *= __mainColor.rgb;
				emission += __emission;

				// main light: direction, color, distanceAttenuation, shadowAttenuation
			#ifdef _MAIN_LIGHT_SHADOWS
				Light mainLight = GetMainLight(input.shadowCoord);
			#else
				Light mainLight = GetMainLight();
			#endif

				half3 lightDir = mainLight.direction;
				half3 lightColor = mainLight.color.rgb;
				half atten = mainLight.shadowAttenuation;

				half ndl = max(0, dot(normalWS, lightDir));
				half3 ramp;
				half rampThreshold = __rampThreshold;
				half rampSmooth = __rampSmoothing * 0.5;
				ndl = saturate(ndl);
				ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);
				fixed3 rampGrayscale = ramp;

				// apply attenuation
				ramp *= atten;

				// highlight/shadow colors
				ramp = lerp(__shadowColor, __highlightColor, ramp);

				// output color
				half3 color = half3(0,0,0);
				// Rim Lighting
				half rim = 1 - ndvRaw;
				half rimMin = __rimMin;
				half rimMax = __rimMax;
				rim = smoothstep(rimMin, rimMax, rim);
				half3 rimColor = __rimColor;
				half rimStrength = __rimStrength;
				//Rim light mask
				color.rgb += ndl * atten * rim * rimColor * rimStrength;
				color += albedo * lightColor.rgb * ramp;

				//Specular: PBR Blinn-Phong
				half3 halfDir = SafeNormalize(lightDir + viewDirWS);
				half roughness = __specularSmoothnessPbr*__specularSmoothnessPbr;
				half nh = saturate(dot(normalWS, halfDir));
				half spec = NDFBlinnPhong(nh, PercRoughnessToSpecPower(roughness));
				spec *= atten;
				
				//Apply specular
				color.rgb += spec * lightColor.rgb * __specularColor;

				// Additional lights loop
			#ifdef _ADDITIONAL_LIGHTS
				int additionalLightsCount = GetAdditionalLightsCount();
				for (int i = 0; i < additionalLightsCount; ++i)
				{
					Light light = GetAdditionalLight(i, positionWS);
					half atten = light.shadowAttenuation * light.distanceAttenuation;
					half3 lightDir = light.direction;
					half3 lightColor = light.color.rgb;

					half ndl = max(0, dot(normalWS, lightDir));
					half3 ramp;
					ndl = saturate(ndl);
					ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

					// apply attenuation (shadowmaps & point/spot lights attenuation)
					ramp *= atten;

					// apply highlight color
					ramp = lerp(half3(0,0,0), __highlightColor, ramp);

					// output color
					color += albedo * lightColor.rgb * ramp;

					//Specular: PBR Blinn-Phong
					half3 halfDir = SafeNormalize(lightDir + viewDirWS);
					half roughness = __specularSmoothnessPbr*__specularSmoothnessPbr;
					half nh = saturate(dot(normalWS, halfDir));
					half spec = NDFBlinnPhong(nh, PercRoughnessToSpecPower(roughness));
					spec *= atten;
					
					//Apply specular
					color.rgb += spec * lightColor.rgb * __specularColor;
					// Rim light mask
					half3 rimColor = __rimColor;
					half rimStrength = __rimStrength;
					color.rgb += ndl * atten * rim * rimColor * rimStrength;
				}
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				color += input.vertexLights * albedo;
			#endif

				// ambient or lightmap
				// Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
				// are also defined in case you want to sample some terms per-vertex.
				half3 bakedGI = SampleSH(normalWS);
				half occlusion = 1;
				half3 indirectDiffuse = bakedGI;
				indirectDiffuse *= occlusion * albedo * __ambientIntensity;
				color += indirectDiffuse;

				color += emission;

				return half4(color, alpha);
			}
			ENDHLSL
		}

		// Depth & Shadow Caster Passes
		HLSLINCLUDE
		#if defined(SHADOW_CASTER_PASS) || defined(DEPTH_ONLY_PASS)

			#define fixed half
			#define fixed2 half2
			#define fixed3 half3
			#define fixed4 half4

			float3 _LightDirection;

			CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _BaseColor;
			half3 _Emission;
			float _RampThreshold;
			float _RampSmoothing;
			fixed3 _SColor;
			fixed3 _HColor;
			float _RimMin;
			float _RimMax;
			fixed3 _RimColor;
			float _SpecularSmoothnessPBR;
			fixed3 _SpecularColor;
			
			//Specular help functions (from UnityStandardBRDF.cginc)
			inline half3 SafeNormalize(half3 inVec)
			{
				half dp3 = max(0.001f, dot(inVec, inVec));
				return inVec * rsqrt(dp3);
			}
			
			//PBR Blinn-Phong
			#define TCP2_PI 3.14159265359
			inline half PercRoughnessToSpecPower(half roughness)
			{
				half sq = max(1e-4f, roughness*roughness);
				half n = (2.0 / sq) - 2.0;
				n = max(n, 1e-4f);
				return n;
			}
			inline half NDFBlinnPhong(half NdotH, half n)
			{
				// norm = (n+2)/(2*pi)
				half normTerm = (n + 2.0) * (0.5/TCP2_PI);
			
				half specTerm = pow (NdotH, n);
				return specTerm * normTerm;
			}
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				half4 vertexColor : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float4 vertexColor : TEXCOORD0;
				float2 pack1 : TEXCOORD1; /* pack1.xy = texcoord0 */
			#if defined(DEPTH_ONLY_PASS)
				UNITY_VERTEX_OUTPUT_STEREO
			#endif
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

			#if UNITY_REVERSED_Z
				positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#else
				positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#endif

				return positionCS;
			}

			Varyings ShadowDepthPassVertex(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
			#if defined(DEPTH_ONLY_PASS)
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
			#endif
				// Texture Coordinates
				output.pack1.xy.xy = (input.texcoord0.xy) * _MainTex_ST.xy + _MainTex_ST.zw;

				output.vertexColor = input.vertexColor;

			#if defined(DEPTH_ONLY_PASS)
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
			#elif defined(SHADOW_CASTER_PASS)
				output.positionCS = GetShadowPositionHClip(input);
			#else
				output.positionCS = float4(0,0,0,0);
			#endif

				return output;
			}

			half4 ShadowDepthPassFragment(Varyings input) : SV_TARGET
			{
				// Shader Properties Sampling
				float4 __albedo = ( tex2D(_MainTex, (input.pack1.xy.xy)).rgba * input.vertexColor.rgba );
				float4 __mainColor = ( _BaseColor.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float3 __emission = ( _Emission.rgb * input.vertexColor.aaa );
				float __rampThreshold = ( _RampThreshold );
				float __rampSmoothing = ( _RampSmoothing );
				float3 __shadowColor = ( _SColor.rgb );
				float3 __highlightColor = ( _HColor.rgb );
				float __rimMin = ( _RimMin );
				float __rimMax = ( _RimMax );
				float3 __rimColor = ( _RimColor.rgb );
				float __rimStrength = ( 1.0 );
				float __specularSmoothnessPbr = (  1 - __albedo.a * _SpecularSmoothnessPBR );
				float3 __specularColor = ( _SpecularColor.rgb );
				float __ambientIntensity = ( 1.0 );

				half3 albedo = __albedo.rgb;
				half alpha = __alpha;
				half3 emission = half3(0,0,0);
				return 0;
			}

		#endif
		ENDHLSL

		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile SHADOW_CASTER_PASS

			// -------------------------------------
			// Material Keywords
			//#pragma shader_feature _ALPHATEST_ON
			//#pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Shadows.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0

			HLSLPROGRAM

			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// -------------------------------------
			// Material Keywords
			// #pragma shader_feature _ALPHATEST_ON
			// #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile DEPTH_ONLY_PASS

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			ENDHLSL
		}

		// Depth prepass
		// UsePass "Lightweight Render Pipeline/Lit/DepthOnly"

		// Used for Baking GI. This pass is stripped from build.
		UsePass "Lightweight Render Pipeline/Lit/Meta"
	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.4.0";tmplt:"SG2_Template_LWRP";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","SPEC_PBR_BLINNPHONG","SPECULAR","RIM","RIM_LIGHTMASK","EMISSION","SHADOW_COLOR_MAIN_DIR","TEMPLATE_LWRP"];flags:list[];keywords:dict[RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0",RIM_LABEL="Rim Lighting",RENDER_TYPE="Opaque"];shaderProperties:list[sp(name:"Albedo";imps:list[imp_mp_texture(guid:"4030e6dc-614f-4a02-b744-6c40b441e248";uto:True;tov:"";gto:True;sbt:False;scr:False;scv:"";gsc:False;roff:False;goff:False;notile:False;def:"white";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv:False;ssuv_vert:False;ssuv_obj:False;prop:"_MainTex";md:"";custom:False;refs:"";op:Multiply;lbl:"Albedo";gpu_inst:False;locked:False;impl_index:-1),imp_vcolors(cc:4;chan:"RGBA";op:Multiply;lbl:"Albedo";gpu_inst:False;locked:False;impl_index:-1)]),,,,,,,,,sp(name:"Specular Smoothness PBR";imps:list[imp_customcode(code:"1 - {2}.a";op:Multiply;lbl:"Specular Smoothness PBR";gpu_inst:False;locked:False;impl_index:-1),imp_spref(cc:1;chan:"A";lsp:"Albedo";op:Multiply;lbl:"Specular Smoothness PBR";gpu_inst:False;locked:False;impl_index:-1),imp_mp_range(def:0.5;min:0;max:1;prop:"_SpecularSmoothnessPBR";md:"";custom:False;refs:"";op:Multiply;lbl:"Smoothness";gpu_inst:False;locked:False;impl_index:-1)]),,,,,sp(name:"Emission";imps:list[imp_mp_color(def:RGBA(0.000, 0.000, 0.000, 1.000);hdr:True;cc:3;chan:"RGB";prop:"_Emission";md:"";custom:False;refs:"";op:Multiply;lbl:"Emission Color";gpu_inst:False;locked:False;impl_index:-1),imp_vcolors(cc:3;chan:"AAA";op:Multiply;lbl:"Emission";gpu_inst:False;locked:False;impl_index:-1)])];customTextures:list[]) */
/* TCP_HASH 283e47a1b3df982bab5641d8ba53dc3d */
