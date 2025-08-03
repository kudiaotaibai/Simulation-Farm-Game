// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/Water/WaterSample"
{
	Properties
	{
		[Header(Refraction)]
		_ChromaticAberration("Chromatic Aberration", Range( 0 , 0.3)) = 0.1
		_WaterNormal("Water Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Float) = 0
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_ShalowColor("Shalow Color", Color) = (1,1,1,0)
		_WaterDepth("Water Depth", Float) = 0
		_WaterFalloff("Water Falloff", Float) = 0
		_WaterSpecular("Water Specular", Float) = 0
		_WaterSmoothness("Water Smoothness", Float) = 0
		_Foam("Foam", 2D) = "white" {}
		_FoamDepth("Foam Depth", Float) = 0
		_FoamFalloff("Foam Falloff", Float) = 0
		_FoamSpecular("Foam Specular", Float) = 0
		_FoamSmoothness("Foam Smoothness", Float) = 0
		_Color_E("Color_E", Color) = (0,0,0,0)
		_Opacity("Opacity", Float) = 1
		_Refraction("Refraction", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma multi_compile _ALPHAPREMULTIPLY_ON
		#pragma surface surf StandardSpecular alpha:fade keepalpha finalcolor:RefractionF exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
		};

		uniform sampler2D _WaterNormal;
		uniform float _NormalScale;
		uniform float4 _WaterNormal_ST;
		uniform float4 _DeepColor;
		uniform float4 _ShalowColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _WaterDepth;
		uniform float _WaterFalloff;
		uniform half4 _Color_E;
		uniform float _WaterSpecular;
		uniform float _FoamSpecular;
		uniform float _FoamDepth;
		uniform float _FoamFalloff;
		uniform sampler2D _Foam;
		uniform float4 _Foam_ST;
		uniform float _WaterSmoothness;
		uniform float _FoamSmoothness;
		uniform half _Opacity;
		uniform sampler2D _GrabTexture;
		uniform float _ChromaticAberration;
		uniform half _Refraction;

		inline float4 Refraction( Input i, SurfaceOutputStandardSpecular o, float indexOfRefraction, float chomaticAberration ) {
			float3 worldNormal = o.Normal;
			float4 screenPos = i.screenPos;
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			float halfPosW = screenPos.w * 0.5;
			screenPos.y = ( screenPos.y - halfPosW ) * _ProjectionParams.x * scale + halfPosW;
			#if SHADER_API_D3D9 || SHADER_API_D3D11
				screenPos.w += 0.00000000001;
			#endif
			float2 projScreenPos = ( screenPos / screenPos.w ).xy;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 refractionOffset = ( ( ( ( indexOfRefraction - 1.0 ) * mul( UNITY_MATRIX_V, float4( worldNormal, 0.0 ) ) ) * ( 1.0 / ( screenPos.z + 1.0 ) ) ) * ( 1.0 - dot( worldNormal, worldViewDir ) ) );
			float2 cameraRefraction = float2( refractionOffset.x, -( refractionOffset.y * _ProjectionParams.x ) );
			float4 redAlpha = tex2D( _GrabTexture, ( projScreenPos + cameraRefraction ) );
			float green = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 - chomaticAberration ) ) ) ).g;
			float blue = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 + chomaticAberration ) ) ) ).b;
			return float4( redAlpha.r, green, blue, redAlpha.a );
		}

		void RefractionF( Input i, SurfaceOutputStandardSpecular o, inout half4 color )
		{
			#ifdef UNITY_PASS_FORWARDBASE
			color.rgb = color.rgb + Refraction( i, o, _Refraction, _ChromaticAberration ) * ( 1 - color.a );
			color.a = 1;
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			o.Normal = float3(0,0,1);
			float2 uv0_WaterNormal = i.uv_texcoord * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			half2 panner22 = ( 1.0 * _Time.y * float2( -0.2,0 ) + uv0_WaterNormal);
			half2 panner19 = ( 1.0 * _Time.y * float2( 0.04,0.04 ) + uv0_WaterNormal);
			o.Normal = BlendNormals( UnpackScaleNormal( tex2D( _WaterNormal, panner22 ), _NormalScale ) , UnpackScaleNormal( tex2D( _WaterNormal, panner19 ), _NormalScale ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			half eyeDepth1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half temp_output_89_0 = abs( ( eyeDepth1 - ase_screenPos.w ) );
			half4 lerpResult13 = lerp( _DeepColor , _ShalowColor , saturate( pow( ( temp_output_89_0 + _WaterDepth ) , _WaterFalloff ) ));
			o.Albedo = lerpResult13.rgb;
			o.Emission = _Color_E.rgb;
			float2 uv0_Foam = i.uv_texcoord * _Foam_ST.xy + _Foam_ST.zw;
			half2 panner116 = ( 1.0 * _Time.y * float2( -0.01,0.01 ) + uv0_Foam);
			half temp_output_114_0 = ( saturate( pow( ( temp_output_89_0 + _FoamDepth ) , _FoamFalloff ) ) * tex2D( _Foam, panner116 ).r );
			half lerpResult130 = lerp( _WaterSpecular , _FoamSpecular , temp_output_114_0);
			half3 temp_cast_2 = (lerpResult130).xxx;
			o.Specular = temp_cast_2;
			half lerpResult133 = lerp( _WaterSmoothness , _FoamSmoothness , temp_output_114_0);
			o.Smoothness = lerpResult133;
			o.Alpha = _Opacity;
			o.Normal = o.Normal + 0.00001 * i.screenPos * i.worldPos;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17500
-1443.333;202;1344;533;-1083.175;640.7926;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;152;-2053.601,-256.6997;Inherit;False;843.903;553.8391;Screen depth difference to get intersection and fading effect with terrain and objects;5;89;3;1;2;166;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;166;-2010.745,-176.8604;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;2;-1993.601,-9.1996;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;1;-1781.601,-155.6997;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-1574.201,-110.3994;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;89;-1389.004,-112.5834;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;155;-1106.507,7.515848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;153;-843.9032,402.718;Inherit;False;1083.102;484.2006;Foam controls and texture;9;116;105;106;115;111;110;112;113;114;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;154;-922.7065,390.316;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-722.2024,526.6185;Float;False;Property;_FoamDepth;Foam Depth;11;0;Create;True;0;0;False;0;0;1.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;159;-863.7005,-467.5007;Inherit;False;1113.201;508.3005;Depths controls and colors;10;87;94;12;13;156;157;11;88;10;6;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-531.4025,588.5187;Float;False;Property;_FoamFalloff;Foam Falloff;12;0;Create;True;0;0;False;0;0;-45.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-542.0016,452.718;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;106;-793.9032,700.119;Inherit;False;0;105;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-813.7005,-128.1996;Float;False;Property;_WaterDepth;Water Depth;6;0;Create;True;0;0;False;0;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;158;-1075.608,-163.0834;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;151;-935.9057,-1082.484;Inherit;False;1281.603;457.1994;Blend panning normals to fake noving ripples;7;19;23;24;21;22;17;48;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PowerNode;110;-357.2024,461.6185;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-636.2005,-79.20019;Float;False;Property;_WaterFalloff;Water Falloff;7;0;Create;True;0;0;False;0;0;-0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-632.0056,-204.5827;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-936.9058,-1015.185;Inherit;False;0;17;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;116;-573.2014,720.3181;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.01,0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;105;-304.4021,674.9185;Inherit;True;Property;_Foam;Foam;10;0;Create;True;0;0;False;0;-1;None;d01457b88b1c5174ea4235d140b5fab8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;87;-455.8059,-118.1832;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-697.5002,-417.5007;Float;False;Property;_DeepColor;Deep Color;4;0;Create;True;0;0;False;0;0,0,0,0;0.3764706,0.4470588,0.4705882,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;48;-557.3063,-795.3858;Float;False;Property;_NormalScale;Normal Scale;3;0;Create;True;0;0;False;0;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;113;-136.0011,509.618;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;22;-613.2062,-1032.484;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.2,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;11;-455.0999,-328.3;Float;False;Property;_ShalowColor;Shalow Color;5;0;Create;True;0;0;False;0;1,1,1,0;0.8901961,0.9882353,1,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;19;-610.9061,-919.9849;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.04,0.04;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;26;920.1959,-279.1855;Float;False;Property;_WaterSmoothness;Water Smoothness;9;0;Create;True;0;0;False;0;0;0.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;80.19891,604.0181;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;756.1969,-467.1806;Float;False;Property;_FoamSpecular;Foam Specular;13;0;Create;True;0;0;False;0;0;0.11;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;753.9969,-565.4819;Float;False;Property;_WaterSpecular;Water Specular;8;0;Create;True;0;0;False;0;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;23;-269.2061,-1024.185;Inherit;True;Property;_Normal2;Normal2;2;0;Create;True;0;0;False;0;-1;None;None;True;0;True;bump;Auto;True;Instance;17;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;17;-256.3054,-814.2847;Inherit;True;Property;_WaterNormal;Water Normal;2;0;Create;True;0;0;False;0;-1;None;dd2fd2df93418444c8e280f1d34deeb5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;94;-249.5044,-96.98394;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;156;-151.0076,-354.5834;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;132;914.3978,-199.48;Float;False;Property;_FoamSmoothness;Foam Smoothness;14;0;Create;True;0;0;False;0;0;-0.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;157;-149.1077,-261.9834;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;133;1139.597,-182.68;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;169;1393.914,-631.0501;Inherit;False;Property;_Color_E;Color_E;15;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;130;955.7971,-465.8806;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;170;1833.22,-449.0558;Inherit;False;Property;_Opacity;Opacity;16;0;Create;True;0;0;False;0;1;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;24;170.697,-879.6849;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;13;60.50008,-220.6998;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;176;1742.832,-348.9141;Inherit;False;Property;_Refraction;Refraction;17;0;Create;True;0;0;False;0;0;1.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2032.216,-736.3475;Half;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;ASESampleShaders/Water/WaterSample;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;0;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.01;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;0;166;0
WireConnection;3;0;1;0
WireConnection;3;1;2;4
WireConnection;89;0;3;0
WireConnection;155;0;89;0
WireConnection;154;0;155;0
WireConnection;115;0;154;0
WireConnection;115;1;111;0
WireConnection;158;0;89;0
WireConnection;110;0;115;0
WireConnection;110;1;112;0
WireConnection;88;0;158;0
WireConnection;88;1;6;0
WireConnection;116;0;106;0
WireConnection;105;1;116;0
WireConnection;87;0;88;0
WireConnection;87;1;10;0
WireConnection;113;0;110;0
WireConnection;22;0;21;0
WireConnection;19;0;21;0
WireConnection;114;0;113;0
WireConnection;114;1;105;1
WireConnection;23;1;22;0
WireConnection;23;5;48;0
WireConnection;17;1;19;0
WireConnection;17;5;48;0
WireConnection;94;0;87;0
WireConnection;156;0;12;0
WireConnection;157;0;11;0
WireConnection;133;0;26;0
WireConnection;133;1;132;0
WireConnection;133;2;114;0
WireConnection;130;0;104;0
WireConnection;130;1;131;0
WireConnection;130;2;114;0
WireConnection;24;0;23;0
WireConnection;24;1;17;0
WireConnection;13;0;156;0
WireConnection;13;1;157;0
WireConnection;13;2;94;0
WireConnection;0;0;13;0
WireConnection;0;1;24;0
WireConnection;0;2;169;0
WireConnection;0;3;130;0
WireConnection;0;4;133;0
WireConnection;0;8;176;0
WireConnection;0;9;170;0
ASEEND*/
//CHKSM=3844AE9077AF27C51BE688CFECC7CB1034CEA925