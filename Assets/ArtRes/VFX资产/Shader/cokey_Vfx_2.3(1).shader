// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "cokey/Vfx_2.3"
{
	Properties
	{
		[Enum(Alpha,10,Add,1)]_Float8("混合模式", Float) = 1
		_Float15("软粒子强度", Float) = 0
		_noise_qiangdu("空扭强度", Range( -0.3 , 0.3)) = 0
		_MainTextures("▬▬▬▬▬▬▬ 表面贴图（空扭贴图） ▬▬▬▬▬▬▬▬▬", 2D) = "white" {}
		[HDR]_FreiColor1("表面色", Color) = (1,1,1,1)
		[Toggle]_ToggleSwitch0("自身通道", Float) = 0
		[Toggle]_Float2("表面-一次性UV开关", Float) = 0
		_Float0("U平移", Float) = 0
		_Float1("V平移", Float) = 0
		[Toggle]_ToggleSwitch5("U贴图重铺关闭", Float) = 0
		[Toggle]_ToggleSwitch6("V贴图重铺关闭", Float) = 0
		_Float27("旋转角度", Float) = 0
		_TextureMask("▬▬▬▬▬▬▬ 遮罩贴图（空扭曲遮罩） ▬▬▬▬▬▬▬▬▬", 2D) = "white" {}
		_Float3("U平移-遮罩", Float) = 0
		_Float5("V平移-遮罩", Float) = 0
		_Float31("遮罩1贴图旋转角度", Float) = 0
		_TextureMask2("▬▬▬▬▬▬▬ 遮罩贴图2 ▬▬▬▬▬▬▬▬▬", 2D) = "white" {}
		_Float34("U平移-遮罩2", Float) = 0
		_Float33("V平移-遮罩2", Float) = 0
		_DissTextuers("▬▬▬▬▬▬▬ 溶解贴图 ▬▬▬▬▬▬▬▬▬", 2D) = "white" {}
		[Toggle]_ToggleSwitch7("溶解开关", Float) = 0
		[Toggle]_Float35("溶解-UV一次性流动开关", Float) = 0
		_Float10("溶解软边", Range( 0 , 2)) = 0
		_Float7("U平移-溶解", Float) = 0
		_Float9("V平移-溶解", Float) = 0
		[Toggle]_ToggleSwitch4("溶解亮边开关", Float) = 0
		_Float14("溶解亮边宽度", Range( 0 , 0.5)) = 0.1
		_DissLinePower("溶解亮边强度", Float) = 1
		[HDR]_FreiColor2("溶解边颜色", Color) = (1,1,1,1)
		[Toggle]_dissPoweror("用下面的数值控制溶解", Float) = 0
		_Float30("溶解值", Float) = 0
		_TextureDistor("▬▬▬▬▬▬▬ UV扭曲 ▬▬▬▬▬▬▬▬▬", 2D) = "black" {}
		_Float13("uv扭曲强度", Float) = 0
		_Float11("U平移-扭曲uv", Float) = 0
		_Float12("V平移-扭曲uv", Float) = 0
		[Toggle]_ToggleSwitch8ff("CustomY控制扭曲强度(跟模型变形共用)", Float) = 0
		[Toggle]_ToggleSwitch10("扭曲影响遮罩贴图", Float) = 0
		[Toggle]_ToggleSwitch11("扭曲影响溶解贴图", Float) = 0
		_TextureSample0("▬▬▬▬▬▬▬  叠色贴图 ▬▬▬▬▬▬▬▬▬", 2D) = "white" {}
		_TextureVertexoff("▬▬▬▬▬▬▬ 顶点变形 ▬▬▬▬▬▬▬▬▬", 2D) = "black" {}
		[Toggle]_ToggleSwitch1("变形开关（自定义参数Y一定要给值，不然没效果）", Float) = 1
		_Float17("U平移-变形", Float) = 0
		_Float16("V平移-变形", Float) = 0
		_Vector0("三方向强度", Vector) = (0,0,0,0)
		_TextureVertexoff1(" 变形遮罩", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)]_Float4("单双正反面", Float) = 0
		_Float6("背面暗度", Float) = 1
		[Toggle]_ToggleSwitch2("模型边缘透明开关(Fresnel)", Float) = 0
		[Toggle]_ToggleSwitch9("反向透明(Fresnel)", Float) = 0
		_Float20("菲尼尔强度", Float) = 1
		_Float19("菲尼尔宽度", Float) = 1
		[Toggle]_ToggleSwitch12("空气扭曲开关", Float) = 0
		[Toggle]_ToggleSwitch3("模型边缘颜色开关(Fresnel)", Float) = 0
		[HDR]_FreiColor("边缘色", Color) = (1,0.6950364,0.0990566,0)
		_Float22("边缘色宽度", Float) = 1
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] _tex4coord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull [_Float4]
		ZWrite Off
		Blend SrcAlpha [_Float8]
		
		GrabPass{ }
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf StandardCustomLighting keepalpha noshadow nofog vertex:vertexDataFunc 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float4 uv2_tex4coord2;
			float4 vertexColor : COLOR;
			float2 uv3_texcoord3;
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _Float4;
		uniform float _Float8;
		uniform float _ToggleSwitch1;
		uniform sampler2D _TextureVertexoff;
		uniform float _Float17;
		uniform float _Float16;
		uniform float4 _TextureVertexoff_ST;
		uniform float3 _Vector0;
		uniform sampler2D _TextureVertexoff1;
		uniform float4 _TextureVertexoff1_ST;
		uniform float _ToggleSwitch12;
		uniform float _Float6;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform sampler2D _MainTextures;
		uniform float _ToggleSwitch5;
		uniform float _Float0;
		uniform float _Float1;
		uniform float4 _MainTextures_ST;
		uniform float _Float2;
		uniform float _ToggleSwitch6;
		uniform sampler2D _TextureDistor;
		uniform float _Float11;
		uniform float _Float12;
		uniform float4 _TextureDistor_ST;
		uniform float _ToggleSwitch8ff;
		uniform float _Float13;
		uniform float _Float27;
		uniform float4 _FreiColor1;
		uniform float _ToggleSwitch4;
		uniform float _dissPoweror;
		uniform float _Float30;
		uniform float _Float10;
		uniform sampler2D _DissTextuers;
		uniform float _ToggleSwitch11;
		uniform float _Float7;
		uniform float _Float9;
		uniform float4 _DissTextuers_ST;
		uniform float _Float35;
		uniform float _Float14;
		uniform float _DissLinePower;
		uniform float4 _FreiColor2;
		uniform float _ToggleSwitch3;
		uniform float4 _FreiColor;
		uniform float _Float22;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _noise_qiangdu;
		uniform sampler2D _TextureMask;
		uniform float _ToggleSwitch10;
		uniform float _Float3;
		uniform float _Float5;
		uniform float4 _TextureMask_ST;
		uniform float _Float31;
		uniform float _ToggleSwitch0;
		uniform float _ToggleSwitch7;
		uniform float _ToggleSwitch2;
		uniform float _ToggleSwitch9;
		uniform float _Float20;
		uniform float _Float19;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Float15;
		uniform sampler2D _TextureMask2;
		uniform float _Float34;
		uniform float _Float33;
		uniform float4 _TextureMask2_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 temp_cast_0 = (0.0).xxxx;
			float2 appendResult9_g42 = (float2(_Float17 , _Float16));
			float2 uv0_TextureVertexoff = v.texcoord.xy * _TextureVertexoff_ST.xy + _TextureVertexoff_ST.zw;
			float2 panner2_g42 = ( 1.0 * _Time.y * appendResult9_g42 + uv0_TextureVertexoff);
			float3 ase_vertexNormal = v.normal.xyz;
			float2 uv_TextureVertexoff1 = v.texcoord * _TextureVertexoff1_ST.xy + _TextureVertexoff1_ST.zw;
			float4 uv4_TexCoord116 = v.texcoord3;
			uv4_TexCoord116.xy = v.texcoord3.xy + float2( 1,0 );
			v.vertex.xyz += (( _ToggleSwitch1 )?( ( tex2Dlod( _TextureVertexoff, float4( panner2_g42, 0, 0.0) ) * float4( _Vector0 , 0.0 ) * float4( ase_vertexNormal , 0.0 ) * tex2Dlod( _TextureVertexoff1, float4( uv_TextureVertexoff1, 0, 0.0) ).r * uv4_TexCoord116.y ) ):( temp_cast_0 )).rgb;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float2 appendResult9_g28 = (float2(_Float0 , _Float1));
			float2 uv0_MainTextures = i.uv_texcoord * _MainTextures_ST.xy + _MainTextures_ST.zw;
			float2 temp_output_11_0_g28 = uv0_MainTextures;
			float2 panner2_g28 = ( 1.0 * _Time.y * appendResult9_g28 + temp_output_11_0_g28);
			float2 appendResult17_g28 = (float2(i.uv2_tex4coord2.z , i.uv2_tex4coord2.w));
			float2 lerpResult30_g28 = lerp( panner2_g28 , ( temp_output_11_0_g28 + appendResult17_g28 ) , _Float2);
			float2 temp_output_27_0 = lerpResult30_g28;
			float temp_output_169_0 = (temp_output_27_0).x;
			float clampResult167 = clamp( temp_output_169_0 , 0.0 , 1.0 );
			float temp_output_170_0 = (temp_output_27_0).y;
			float clampResult171 = clamp( temp_output_170_0 , 0.0 , 1.0 );
			float2 appendResult172 = (float2((( _ToggleSwitch5 )?( clampResult167 ):( temp_output_169_0 )) , (( _ToggleSwitch6 )?( clampResult171 ):( temp_output_170_0 ))));
			float2 appendResult9_g27 = (float2(_Float11 , _Float12));
			float2 uv0_TextureDistor = i.uv_texcoord * _TextureDistor_ST.xy + _TextureDistor_ST.zw;
			float2 panner2_g27 = ( 1.0 * _Time.y * appendResult9_g27 + uv0_TextureDistor);
			float4 tex2DNode76 = tex2D( _TextureDistor, panner2_g27 );
			float2 appendResult178 = (float2(tex2DNode76.r , tex2DNode76.g));
			float2 UVNiuQu194 = ( appendResult178 * (( _ToggleSwitch8ff )?( i.uv2_tex4coord2.y ):( _Float13 )) );
			float cos149 = cos( ( ( _Float27 * UNITY_PI ) / 180.0 ) );
			float sin149 = sin( ( ( _Float27 * UNITY_PI ) / 180.0 ) );
			float2 rotator149 = mul( ( appendResult172 + UVNiuQu194 ) - float2( 0.5,0.5 ) , float2x2( cos149 , -sin149 , sin149 , cos149 )) + float2( 0.5,0.5 );
			float2 MainTexturesUV69 = rotator149;
			float4 tex2DNode2 = tex2D( _MainTextures, MainTexturesUV69 );
			float2 appendResult9_g40 = (float2(_Float3 , _Float5));
			float2 uv0_TextureMask = i.uv_texcoord * _TextureMask_ST.xy + _TextureMask_ST.zw;
			float2 panner2_g40 = ( 1.0 * _Time.y * appendResult9_g40 + uv0_TextureMask);
			float2 temp_output_37_0 = panner2_g40;
			float cos215 = cos( ( ( _Float31 * UNITY_PI ) / 180.0 ) );
			float sin215 = sin( ( ( _Float31 * UNITY_PI ) / 180.0 ) );
			float2 rotator215 = mul( (( _ToggleSwitch10 )?( ( temp_output_37_0 + UVNiuQu194 ) ):( temp_output_37_0 )) - float2( 0.5,0.5 ) , float2x2( cos215 , -sin215 , sin215 , cos215 )) + float2( 0.5,0.5 );
			float4 tex2DNode23 = tex2D( _TextureMask, rotator215 );
			float2 appendResult9_g39 = (float2(_Float7 , _Float9));
			float2 uv0_DissTextuers = i.uv_texcoord * _DissTextuers_ST.xy + _DissTextuers_ST.zw;
			float2 temp_output_11_0_g39 = uv0_DissTextuers;
			float2 panner2_g39 = ( 1.0 * _Time.y * appendResult9_g39 + temp_output_11_0_g39);
			float2 appendResult17_g39 = (float2(i.uv3_texcoord3.x , i.uv3_texcoord3.y));
			float2 lerpResult30_g39 = lerp( panner2_g39 , ( temp_output_11_0_g39 + appendResult17_g39 ) , _Float35);
			float4 tex2DNode57 = tex2D( _DissTextuers, (( _ToggleSwitch11 )?( ( float2( 0,0 ) + UVNiuQu194 ) ):( lerpResult30_g39 )) );
			float smoothstepResult62 = smoothstep( (( _dissPoweror )?( _Float30 ):( ( 1.0 - i.uv2_tex4coord2.x ) )) , ( (( _dissPoweror )?( _Float30 ):( ( 1.0 - i.uv2_tex4coord2.x ) )) + _Float10 ) , tex2DNode57.r);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV117 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode117 = ( 0.0 + _Float20 * pow( 1.0 - fresnelNdotV117, _Float19 ) );
			float FresnlOpcity125 = (( _ToggleSwitch2 )?( (( _ToggleSwitch9 )?( ( 1.0 - fresnelNode117 ) ):( fresnelNode117 )) ):( 1.0 ));
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth101 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth101 = saturate( abs( ( screenDepth101 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Float15 ) ) );
			float2 appendResult9_g41 = (float2(_Float34 , _Float33));
			float2 uv0_TextureMask2 = i.uv_texcoord * _TextureMask2_ST.xy + _TextureMask2_ST.zw;
			float2 panner2_g41 = ( 1.0 * _Time.y * appendResult9_g41 + uv0_TextureMask2);
			float temp_output_238_0 = ( tex2DNode23.r * i.vertexColor.a );
			c.rgb = 0;
			c.a = (( _ToggleSwitch12 )?( temp_output_238_0 ):( saturate( ( (( _ToggleSwitch0 )?( tex2DNode2.a ):( tex2DNode2.r )) * tex2DNode23.r * (( _ToggleSwitch7 )?( smoothstepResult62 ):( 1.0 )) * FresnlOpcity125 * _FreiColor1.a * i.vertexColor.a * distanceDepth101 * tex2D( _TextureMask2, panner2_g41 ).r ) ) ));
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float2 appendResult9_g28 = (float2(_Float0 , _Float1));
			float2 uv0_MainTextures = i.uv_texcoord * _MainTextures_ST.xy + _MainTextures_ST.zw;
			float2 temp_output_11_0_g28 = uv0_MainTextures;
			float2 panner2_g28 = ( 1.0 * _Time.y * appendResult9_g28 + temp_output_11_0_g28);
			float2 appendResult17_g28 = (float2(i.uv2_tex4coord2.z , i.uv2_tex4coord2.w));
			float2 lerpResult30_g28 = lerp( panner2_g28 , ( temp_output_11_0_g28 + appendResult17_g28 ) , _Float2);
			float2 temp_output_27_0 = lerpResult30_g28;
			float temp_output_169_0 = (temp_output_27_0).x;
			float clampResult167 = clamp( temp_output_169_0 , 0.0 , 1.0 );
			float temp_output_170_0 = (temp_output_27_0).y;
			float clampResult171 = clamp( temp_output_170_0 , 0.0 , 1.0 );
			float2 appendResult172 = (float2((( _ToggleSwitch5 )?( clampResult167 ):( temp_output_169_0 )) , (( _ToggleSwitch6 )?( clampResult171 ):( temp_output_170_0 ))));
			float2 appendResult9_g27 = (float2(_Float11 , _Float12));
			float2 uv0_TextureDistor = i.uv_texcoord * _TextureDistor_ST.xy + _TextureDistor_ST.zw;
			float2 panner2_g27 = ( 1.0 * _Time.y * appendResult9_g27 + uv0_TextureDistor);
			float4 tex2DNode76 = tex2D( _TextureDistor, panner2_g27 );
			float2 appendResult178 = (float2(tex2DNode76.r , tex2DNode76.g));
			float2 UVNiuQu194 = ( appendResult178 * (( _ToggleSwitch8ff )?( i.uv2_tex4coord2.y ):( _Float13 )) );
			float cos149 = cos( ( ( _Float27 * UNITY_PI ) / 180.0 ) );
			float sin149 = sin( ( ( _Float27 * UNITY_PI ) / 180.0 ) );
			float2 rotator149 = mul( ( appendResult172 + UVNiuQu194 ) - float2( 0.5,0.5 ) , float2x2( cos149 , -sin149 , sin149 , cos149 )) + float2( 0.5,0.5 );
			float2 MainTexturesUV69 = rotator149;
			float4 tex2DNode2 = tex2D( _MainTextures, MainTexturesUV69 );
			float4 temp_cast_0 = (0.0).xxxx;
			float2 appendResult9_g39 = (float2(_Float7 , _Float9));
			float2 uv0_DissTextuers = i.uv_texcoord * _DissTextuers_ST.xy + _DissTextuers_ST.zw;
			float2 temp_output_11_0_g39 = uv0_DissTextuers;
			float2 panner2_g39 = ( 1.0 * _Time.y * appendResult9_g39 + temp_output_11_0_g39);
			float2 appendResult17_g39 = (float2(i.uv3_texcoord3.x , i.uv3_texcoord3.y));
			float2 lerpResult30_g39 = lerp( panner2_g39 , ( temp_output_11_0_g39 + appendResult17_g39 ) , _Float35);
			float4 tex2DNode57 = tex2D( _DissTextuers, (( _ToggleSwitch11 )?( ( float2( 0,0 ) + UVNiuQu194 ) ):( lerpResult30_g39 )) );
			float smoothstepResult62 = smoothstep( (( _dissPoweror )?( _Float30 ):( ( 1.0 - i.uv2_tex4coord2.x ) )) , ( (( _dissPoweror )?( _Float30 ):( ( 1.0 - i.uv2_tex4coord2.x ) )) + _Float10 ) , tex2DNode57.r);
			float temp_output_88_0 = ( (( _dissPoweror )?( _Float30 ):( ( 1.0 - i.uv2_tex4coord2.x ) )) + _Float14 );
			float smoothstepResult90 = smoothstep( temp_output_88_0 , ( temp_output_88_0 + _Float10 ) , tex2DNode57.r);
			float3 temp_cast_1 = (0.0).xxx;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV137 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode137 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV137, _Float22 ) );
			float3 clampResult203 = clamp( ( (_FreiColor).rgb * fresnelNode137 ) , float3( 0,0,0 ) , float3( 6,6,6 ) );
			float3 FresnelCOlor133 = (( _ToggleSwitch3 )?( clampResult203 ):( temp_cast_1 ));
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 temp_cast_4 = (tex2DNode2.r).xx;
			float2 lerpResult234 = lerp( (ase_screenPosNorm).xy , temp_cast_4 , _noise_qiangdu);
			float4 screenColor236 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,lerpResult234);
			float2 appendResult9_g40 = (float2(_Float3 , _Float5));
			float2 uv0_TextureMask = i.uv_texcoord * _TextureMask_ST.xy + _TextureMask_ST.zw;
			float2 panner2_g40 = ( 1.0 * _Time.y * appendResult9_g40 + uv0_TextureMask);
			float2 temp_output_37_0 = panner2_g40;
			float cos215 = cos( ( ( _Float31 * UNITY_PI ) / 180.0 ) );
			float sin215 = sin( ( ( _Float31 * UNITY_PI ) / 180.0 ) );
			float2 rotator215 = mul( (( _ToggleSwitch10 )?( ( temp_output_37_0 + UVNiuQu194 ) ):( temp_output_37_0 )) - float2( 0.5,0.5 ) , float2x2( cos215 , -sin215 , sin215 , cos215 )) + float2( 0.5,0.5 );
			float4 tex2DNode23 = tex2D( _TextureMask, rotator215 );
			float temp_output_238_0 = ( tex2DNode23.r * i.vertexColor.a );
			float4 appendResult240 = (float4((screenColor236).rgb , temp_output_238_0));
			o.Emission = (( _ToggleSwitch12 )?( appendResult240 ):( ( _Float6 * ( ( tex2D( _TextureSample0, uv_TextureSample0 ) * tex2DNode2 * _FreiColor1 * i.vertexColor ) + (( _ToggleSwitch4 )?( ( ( smoothstepResult62 - smoothstepResult90 ) * _DissLinePower * _FreiColor2 ) ):( temp_cast_0 )) + float4( FresnelCOlor133 , 0.0 ) ) ) )).xyz;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
0;5;1920;1014;-3029.918;1251.077;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;87;-2479.096,-2335.38;Inherit;False;3106.657;1087.222;MainUV;29;69;149;153;155;77;154;151;82;27;4;5;152;29;3;76;150;83;11;78;80;81;79;170;171;178;173;182;194;195;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-2426.096,-1474.911;Inherit;False;Property;_Float12;V平移-扭曲uv;34;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-2411.096,-1572.911;Inherit;False;Property;_Float11;U平移-扭曲uv;33;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-2430.116,-1731.911;Inherit;False;0;76;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;29;-2161.777,-2153.501;Inherit;False;Property;_Float2;表面-一次性UV开关;6;1;[Toggle];Create;False;2;Close;0;Open;1;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-2151.684,-1983.912;Inherit;False;Property;_Float1;V平移;8;0;Create;False;0;0;False;0;0;0.93;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-2294.381,-2280.38;Inherit;False;0;2;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-2309.729,-1906.137;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-2145.136,-2066.234;Inherit;False;Property;_Float0;U平移;7;0;Create;False;0;0;False;0;0;-0.06;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;78;-2175.031,-1599.04;Inherit;False;UVPanSimple;-1;;27;5938cf4db0f98ca41a6be0775ee84d0e;0;3;11;FLOAT2;0,0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;76;-1860.746,-1807.208;Inherit;True;Property;_TextureDistor;▬▬▬▬▬▬▬ UV扭曲 ▬▬▬▬▬▬▬▬▬;31;0;Create;False;0;0;False;0;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;27;-1827.797,-2110.771;Inherit;False;UVPan;-1;;28;1445317dd62ca0c46851686f0caf0003;0;6;11;FLOAT2;0,0;False;31;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;10;FLOAT;0;False;15;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-1682.366,-1554.154;Inherit;False;Property;_Float13;uv扭曲强度;32;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;170;-1461.055,-2320.059;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;169;-1479.071,-2478.657;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;182;-1543.03,-1460.63;Inherit;False;Property;_ToggleSwitch8ff;CustomY控制扭曲强度(跟模型变形共用);35;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;178;-1512.141,-1805.188;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;167;-1247.351,-2422.594;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;171;-1256.955,-2203.56;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;82;-1361.537,-1739.714;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;71;-1002.692,1063.831;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;150;-941.4868,-1728.724;Inherit;False;Property;_Float27;旋转角度;11;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;152;-938.4868,-1609.724;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;194;-1178.939,-1619.065;Inherit;False;UVNiuQu;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;168;-1052.807,-2524.438;Inherit;False;Property;_ToggleSwitch5;U贴图重铺关闭;9;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;173;-1045.984,-2295.494;Inherit;False;Property;_ToggleSwitch6;V贴图重铺关闭;10;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;154;-750.4865,-1564.724;Inherit;False;Constant;_Float28;Float 28;39;0;Create;True;0;0;False;0;180;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-2013.183,805.501;Inherit;False;Property;_Float7;U平移-溶解;23;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;226;-1829.022,742.7097;Inherit;False;Property;_Float35;溶解-UV一次性流动开关;21;1;[Toggle];Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-2027.582,959.6988;Inherit;False;Property;_Float9;V平移-溶解;24;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;172;-819.1,-2428.292;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;59;-2068.777,611.5076;Inherit;False;0;57;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;195;-1117.116,-1946.47;Inherit;False;194;UVNiuQu;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;199;-1469.837,1236.417;Inherit;False;194;UVNiuQu;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;68;-750.8552,1023.385;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-716.0364,1144.736;Inherit;False;Property;_Float30;溶解值;30;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;225;-2283.459,1131.359;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;156;-2698.449,-526.5594;Inherit;False;1173;1064.393;feinier;18;134;124;123;139;132;142;130;137;117;129;143;144;125;136;133;190;191;203;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;151;-721.4865,-1765.724;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;153;-515.4863,-1681.724;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;198;-1217.167,1179.891;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;123;-2672.452,396.5967;Inherit;False;Property;_Float19;菲尼尔宽度;51;0;Create;False;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;155;-900.4866,-1876.724;Inherit;False;Constant;_Vector1;Vector 1;39;0;Create;True;0;0;False;0;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;134;-2640.449,-135.5091;Inherit;False;Constant;_Float23;边缘色强度;32;0;Create;False;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-2682.597,263.1331;Inherit;False;Property;_Float20;菲尼尔强度;50;0;Create;False;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-883.3253,-2048.523;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-91.17238,1073.426;Inherit;False;Property;_Float14;溶解亮边宽度;26;0;Create;False;0;0;False;0;0.1;0.1;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;100;-1333.215,5.241922;Inherit;False;1251.238;423;MaskTextures;8;33;32;34;23;37;192;193;196;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;132;-2648.449,-58.50906;Inherit;False;Property;_Float22;边缘色宽度;55;0;Create;False;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;221;-1421.606,682.1923;Inherit;False;UVPan;-1;;39;5b182a1b7411be143844e9fa49ee5dc9;0;6;11;FLOAT2;0,0;False;31;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;10;FLOAT;0;False;15;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;139;-2691.352,-471.5594;Inherit;False;Property;_FreiColor;边缘色;54;1;[HDR];Create;False;0;0;False;0;1,0.6950364,0.0990566,0;1,0.6950364,0.0990566,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;201;-580.0364,1023.736;Inherit;False;Property;_dissPoweror;用下面的数值控制溶解;29;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-1323.582,36.84519;Inherit;False;0;23;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;32;-1190.515,179.9418;Inherit;False;Property;_Float3;U平移-遮罩;13;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;117;-2490.7,247.521;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1234.215,284.2417;Inherit;False;Property;_Float5;V平移-遮罩;14;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;197;-1132.867,938.8909;Inherit;False;Property;_ToggleSwitch11;扭曲影响溶解贴图;37;0;Create;False;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;142;-2444.84,-414.9272;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RotatorNode;149;-616.9135,-1929.307;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FresnelNode;137;-2454.449,-175.5091;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-568.0497,1194.901;Inherit;False;Property;_Float10;溶解软边;22;0;Create;False;0;0;False;0;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;193.3925,1010.767;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;210;-1107.7,-187.2125;Inherit;False;Property;_Float31;遮罩1贴图旋转角度;15;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;37;-943.9138,122.7543;Inherit;False;UVPanSimple;-1;;40;5938cf4db0f98ca41a6be0775ee84d0e;0;3;11;FLOAT2;0,0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;190;-2212.041,362.4129;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;196;-956.1498,352.439;Inherit;False;194;UVNiuQu;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;241;1370.976,-1888.519;Inherit;False;2219.033;1286.612;niuq;9;230;231;232;234;235;236;238;239;240;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;57;-899.848,746.7083;Inherit;True;Property;_DissTextuers;▬▬▬▬▬▬▬ 溶解贴图 ▬▬▬▬▬▬▬▬▬;19;0;Create;False;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-307.7604,900.2303;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;211;-1104.7,-68.21252;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;91;325.1652,1115.968;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;144;-2170.84,-188.9272;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;69;-278.1616,-2015.146;Inherit;True;MainTexturesUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;-887.6995,-224.2125;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;203;-1944.9,-168.5464;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;6,6,6;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-2086.84,-425.9272;Inherit;False;Constant;_Float24;Float 24;36;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;230;1975.223,-1650.488;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;130;-2433.173,92.83202;Inherit;False;Constant;_Float21;Float 21;33;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;213;-916.6995,-23.21252;Inherit;False;Constant;_Float32;Float 32;39;0;Create;True;0;0;False;0;180;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;62;-246.9328,715.3107;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-581.6035,-307.1084;Inherit;False;69;MainTexturesUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;90;463.8924,946.7669;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;191;-2215.071,219.0695;Inherit;False;Property;_ToggleSwitch9;反向透明(Fresnel);49;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;193;-703.4799,295.9128;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;231;2310.619,-937.1639;Inherit;False;Property;_noise_qiangdu;空扭强度;2;0;Create;False;0;0;False;0;0;0.16;-0.3;0.3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;92;614.6652,846.9178;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;214;-681.6993,-140.2125;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;217;-1127.93,438.1231;Inherit;False;0;204;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;220;-1005.263,572.1198;Inherit;False;Property;_Float34;U平移-遮罩2;17;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;208;626.2122,1111.047;Inherit;False;Property;_FreiColor2;溶解边颜色;28;1;[HDR];Create;False;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;218;-1022.963,677.7197;Inherit;False;Property;_Float33;V平移-遮罩2;18;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;136;-1887.244,-370.7363;Inherit;False;Property;_ToggleSwitch3;模型边缘颜色开关(Fresnel);53;0;Create;False;0;0;False;0;0;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;232;2296.798,-1778.609;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;216;-637.0228,-14.27539;Inherit;False;Constant;_Vector2;Vector 2;39;0;Create;True;0;0;False;0;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;97;640.5972,1005.393;Inherit;False;Property;_DissLinePower;溶解亮边强度;27;0;Create;False;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;192;-581.4801,237.9128;Inherit;False;Property;_ToggleSwitch10;扭曲影响遮罩贴图;36;0;Create;False;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;2;-322.7791,-387.1007;Inherit;True;Property;_MainTextures;▬▬▬▬▬▬▬ 表面贴图（空扭贴图） ▬▬▬▬▬▬▬▬▬;3;0;Create;False;0;0;False;0;-1;None;907ecb16122985547a8cfc177df8c24b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;129;-1975.985,171.2087;Inherit;False;Property;_ToggleSwitch2;模型边缘透明开关(Fresnel);48;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;147;145.9992,-299.0082;Inherit;False;Property;_FreiColor1;表面色;4;1;[HDR];Create;False;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;109;-730.1359,1767.063;Inherit;False;Property;_Float17;U平移-变形;41;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;175;-102.7416,613.1182;Inherit;False;Constant;_Float29;Float 29;43;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-1631.403,201.7008;Inherit;False;FresnlOpcity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;162;206.3854,-721.937;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;234;2647.216,-1452.929;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;107;-748.1359,1606.064;Inherit;False;0;103;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;102;384.4069,614.5544;Inherit;False;Property;_Float15;软粒子强度;1;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;99;-165.4862,-634.0201;Inherit;True;Property;_TextureSample0;▬▬▬▬▬▬▬  叠色贴图 ▬▬▬▬▬▬▬▬▬;38;0;Create;False;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;166;672.8764,718.225;Inherit;False;Constant;_Float26;Float 26;40;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;-1642.449,-291.509;Inherit;False;FresnelCOlor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;219;-732.662,516.2323;Inherit;False;UVPanSimple;-1;;41;5938cf4db0f98ca41a6be0775ee84d0e;0;3;11;FLOAT2;0,0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;215;-357.6996,-92.61261;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;869.8093,887.6046;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-745.1359,1864.063;Inherit;False;Property;_Float16;V平移-变形;42;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;235;2819.583,-869.8308;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;204;-315.7604,399.7021;Inherit;True;Property;_TextureMask2;▬▬▬▬▬▬▬ 遮罩贴图2 ▬▬▬▬▬▬▬▬▬;16;0;Create;False;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;23;-258.9782,102.96;Inherit;True;Property;_TextureMask;▬▬▬▬▬▬▬ 遮罩贴图（空扭曲遮罩） ▬▬▬▬▬▬▬▬▬;12;0;Create;False;0;0;False;0;-1;None;0cda140f7a71ef349930a5f3fcf59515;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;53;170.7378,-41.37944;Inherit;False;Property;_ToggleSwitch0;自身通道;5;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;101;567.5143,520.2257;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;145;503.5288,-31.27707;Inherit;False;133;FresnelCOlor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;197.8236,340.0419;Inherit;False;125;FresnlOpcity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;110;-441.0138,1648.13;Inherit;False;UVPanSimple;-1;;42;5938cf4db0f98ca41a6be0775ee84d0e;0;3;11;FLOAT2;0,0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;464.7112,-527.2129;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;174;72.25745,679.1501;Inherit;False;Property;_ToggleSwitch7;溶解开关;20;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;165;1052.635,693.8815;Inherit;False;Property;_ToggleSwitch4;溶解亮边开关;25;0;Create;False;0;0;False;0;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenColorNode;236;3052.951,-1683.111;Inherit;False;Global;_GrabScreen0;Grab Screen 0;0;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;239;3192.113,-1465.171;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;187;961.3046,-345.441;Inherit;False;Property;_Float6;背面暗度;46;0;Create;False;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;96;766.9911,-445.6436;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;238;3177.232,-995.3469;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;103;-29.34798,1570.323;Inherit;True;Property;_TextureVertexoff;▬▬▬▬▬▬▬ 顶点变形 ▬▬▬▬▬▬▬▬▬;39;0;Create;False;0;0;False;0;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;731.6443,110.013;Inherit;False;8;8;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;111;-100.8502,2104.193;Inherit;True;Property;_TextureVertexoff1; 变形遮罩;44;0;Create;False;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;116;-21.85515,2304.754;Inherit;False;3;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;1,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;104;31.49239,1766.776;Inherit;False;Property;_Vector0;三方向强度;43;0;Create;False;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;106;8.652417,1946.545;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;164;1305.772,215.5011;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;409.7604,1770.489;Inherit;False;5;5;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;1117.756,-276.2166;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;115;418.2513,1646.188;Inherit;False;Constant;_Float18;Float 18;30;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;240;3419.009,-1323.34;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;43;1792.304,-351.5957;Inherit;False;Property;_Float8;混合模式;0;1;[Enum];Create;False;2;Alpha;10;Add;1;0;True;0;1;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;1794.035,-254.5145;Inherit;False;Constant;_Float25;深度模式;43;0;Create;False;3;Open;1;Default;0;Close;2;0;True;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;242;3811.044,-992.7244;Inherit;False;Property;_ToggleSwitch12;空气扭曲开关;52;0;Create;False;0;0;False;0;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ToggleSwitchNode;114;665.3403,1654.043;Inherit;False;Property;_ToggleSwitch1;变形开关（自定义参数Y一定要给值，不然没效果）;40;0;Create;False;0;0;False;0;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;40;1800.839,-454.2231;Inherit;False;Property;_Float4;单双正反面;45;1;[Enum];Create;False;0;1;UnityEngine.Rendering.CullMode;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;250;3790.281,-626.2812;Inherit;False;Property;_ToggleSwitch12;空气扭曲开关;46;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;188;1194.604,-39.54944;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FaceVariableNode;183;1038.599,29.1934;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;189;1344.604,3.450562;Inherit;False;Property;_ToggleSwitch8;正反面倒转;47;0;Create;False;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;55;4468.354,-685.412;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;cokey/Vfx_2.3;False;False;False;False;False;False;False;False;False;True;False;False;False;False;True;False;False;False;False;False;False;Back;2;False;148;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Custom;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;42;10;True;43;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;56;-1;-1;-1;0;False;0;0;True;40;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;78;11;81;0
WireConnection;78;6;79;0
WireConnection;78;7;80;0
WireConnection;76;1;78;0
WireConnection;27;11;3;0
WireConnection;27;31;29;0
WireConnection;27;6;4;0
WireConnection;27;7;5;0
WireConnection;27;10;11;3
WireConnection;27;15;11;4
WireConnection;170;0;27;0
WireConnection;169;0;27;0
WireConnection;182;0;83;0
WireConnection;182;1;11;2
WireConnection;178;0;76;1
WireConnection;178;1;76;2
WireConnection;167;0;169;0
WireConnection;171;0;170;0
WireConnection;82;0;178;0
WireConnection;82;1;182;0
WireConnection;194;0;82;0
WireConnection;168;0;169;0
WireConnection;168;1;167;0
WireConnection;173;0;170;0
WireConnection;173;1;171;0
WireConnection;172;0;168;0
WireConnection;172;1;173;0
WireConnection;68;0;71;1
WireConnection;151;0;150;0
WireConnection;151;1;152;0
WireConnection;153;0;151;0
WireConnection;153;1;154;0
WireConnection;198;1;199;0
WireConnection;77;0;172;0
WireConnection;77;1;195;0
WireConnection;221;11;59;0
WireConnection;221;31;226;0
WireConnection;221;6;60;0
WireConnection;221;7;61;0
WireConnection;221;10;225;1
WireConnection;221;15;225;2
WireConnection;201;0;68;0
WireConnection;201;1;202;0
WireConnection;117;2;124;0
WireConnection;117;3;123;0
WireConnection;197;0;221;0
WireConnection;197;1;198;0
WireConnection;142;0;139;0
WireConnection;149;0;77;0
WireConnection;149;1;155;0
WireConnection;149;2;153;0
WireConnection;137;2;134;0
WireConnection;137;3;132;0
WireConnection;88;0;201;0
WireConnection;88;1;89;0
WireConnection;37;11;34;0
WireConnection;37;6;32;0
WireConnection;37;7;33;0
WireConnection;190;0;117;0
WireConnection;57;1;197;0
WireConnection;65;0;201;0
WireConnection;65;1;64;0
WireConnection;91;0;88;0
WireConnection;91;1;64;0
WireConnection;144;0;142;0
WireConnection;144;1;137;0
WireConnection;69;0;149;0
WireConnection;212;0;210;0
WireConnection;212;1;211;0
WireConnection;203;0;144;0
WireConnection;62;0;57;1
WireConnection;62;1;201;0
WireConnection;62;2;65;0
WireConnection;90;0;57;1
WireConnection;90;1;88;0
WireConnection;90;2;91;0
WireConnection;191;0;117;0
WireConnection;191;1;190;0
WireConnection;193;0;37;0
WireConnection;193;1;196;0
WireConnection;92;0;62;0
WireConnection;92;1;90;0
WireConnection;214;0;212;0
WireConnection;214;1;213;0
WireConnection;136;0;143;0
WireConnection;136;1;203;0
WireConnection;232;0;230;0
WireConnection;192;0;37;0
WireConnection;192;1;193;0
WireConnection;2;1;70;0
WireConnection;129;0;130;0
WireConnection;129;1;191;0
WireConnection;125;0;129;0
WireConnection;234;0;232;0
WireConnection;234;1;2;1
WireConnection;234;2;231;0
WireConnection;133;0;136;0
WireConnection;219;11;217;0
WireConnection;219;6;220;0
WireConnection;219;7;218;0
WireConnection;215;0;192;0
WireConnection;215;1;216;0
WireConnection;215;2;214;0
WireConnection;94;0;92;0
WireConnection;94;1;97;0
WireConnection;94;2;208;0
WireConnection;204;1;219;0
WireConnection;23;1;215;0
WireConnection;53;0;2;1
WireConnection;53;1;2;4
WireConnection;101;0;102;0
WireConnection;110;11;107;0
WireConnection;110;6;109;0
WireConnection;110;7;108;0
WireConnection;98;0;99;0
WireConnection;98;1;2;0
WireConnection;98;2;147;0
WireConnection;98;3;162;0
WireConnection;174;0;175;0
WireConnection;174;1;62;0
WireConnection;165;0;166;0
WireConnection;165;1;94;0
WireConnection;236;0;234;0
WireConnection;239;0;236;0
WireConnection;96;0;98;0
WireConnection;96;1;165;0
WireConnection;96;2;145;0
WireConnection;238;0;23;1
WireConnection;238;1;235;4
WireConnection;103;1;110;0
WireConnection;56;0;53;0
WireConnection;56;1;23;1
WireConnection;56;2;174;0
WireConnection;56;3;127;0
WireConnection;56;4;147;4
WireConnection;56;5;162;4
WireConnection;56;6;101;0
WireConnection;56;7;204;1
WireConnection;164;0;56;0
WireConnection;105;0;103;0
WireConnection;105;1;104;0
WireConnection;105;2;106;0
WireConnection;105;3;111;1
WireConnection;105;4;116;2
WireConnection;186;0;187;0
WireConnection;186;1;96;0
WireConnection;240;0;239;0
WireConnection;240;3;238;0
WireConnection;242;0;186;0
WireConnection;242;1;240;0
WireConnection;114;0;115;0
WireConnection;114;1;105;0
WireConnection;250;0;164;0
WireConnection;250;1;238;0
WireConnection;188;0;183;0
WireConnection;189;0;188;0
WireConnection;189;1;183;0
WireConnection;55;2;242;0
WireConnection;55;9;250;0
WireConnection;55;11;114;0
ASEEND*/
//CHKSM=8D203DD520B47130BCDF51C628849D26EA73FAB3