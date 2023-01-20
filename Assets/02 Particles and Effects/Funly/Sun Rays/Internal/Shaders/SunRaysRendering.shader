// Sun Rays by Funly, LLC
// Website: https://funly.io
// Author: Jason Ederle - jason@funly.io

Shader "Hidden/Funly/Sun Rays/Rendering"
{
	Properties
	{
		_MainTex ("Background Texture", 2D) = "white" {}
		_LightTex ("Light Texture", 2D) = "black" {}
		_PatternTex ("Pattern Texture", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	#pragma multi_compile __ BLEND_MODE_ADDITIVE

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	struct v2fBlur
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float2 blurVector : TEXCOORD1;
	};

	sampler2D _MainTex;
	sampler2D _LightTex;
	sampler2D _SkyboxTex;
	sampler2D _PatternTex;
	float _PatternRotationSpeed;
	float _PatternSize;
	float3 _SunViewSpace;
	float _SunShaftRadius;
	fixed4 _ThresholdColor;
	float4 _BlurStepRadius;
	float _Intensity;
	fixed4 _SunColor;
	float _PatternIntensity;
	float _GlobalIntensity;
	
	#define BLUR_PASSES 6
 
	v2f vert (appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}
	
	fixed4 screenBlend(fixed4 c1, fixed4 c2) {
		return 1 - (1 - c1) * (1 - c2);
	}

	float2 rotateAroundPoint(float2 center, float2 pos, float angle) {
		float cosValue = cos(angle);
		float sinValue = sin(angle);

		float2x2 mat = {
			cosValue, -sinValue,
			sinValue, cosValue
		};

		float2 rotatedOriginPos = mul(mat, pos - center);
		return rotatedOriginPos + center;
	}

	fixed transformColor (fixed4 skyboxValue) {
		return dot(max(skyboxValue.rgb - _ThresholdColor.rgb, half3(0,0,0)), half3(1,1,1));
	}
	
	float isInsideBox(float2 pos, float2 bottomLeft, float2 topRight) {
		float2 result = step(bottomLeft, pos) - step(topRight, pos);
		return result.x * result.y;
	}

	fixed4 getPatternValue(float2 uv) {
		half screenHeightRatio = (_ScreenParams.y / _ScreenParams.x);
		half2 screenRatio = half2(screenHeightRatio, 1.0f);
		half2 patternSize = half2(_PatternSize, _PatternSize) * screenRatio;
		half2 patternOrigin = _SunViewSpace.xy - patternSize / 2.0f;
		
		half2 bottomLeftPattern = _SunViewSpace.xy - (patternSize / 2.0f);
		half2 topRightPattern = _SunViewSpace.xy + (patternSize / 2.0f);

		fixed isPattern = isInsideBox(uv, bottomLeftPattern, topRightPattern);

		half2 fragPatternOffset = uv - patternOrigin;
		half2 fragPatternUV = fragPatternOffset / patternSize;
		
		half2 rotatedPatternUV1 = rotateAroundPoint(float2(.5f, .5f), fragPatternUV, _Time.y * _PatternRotationSpeed);

		fixed4 outColor = tex2D(_PatternTex, rotatedPatternUV1);

		// Convert to grayscale.
		fixed patternValue = dot(outColor.xyz, half3(1, 1, 1));
		outColor = half4(patternValue, patternValue, patternValue, outColor.a) * isPattern;

		return outColor;
	}

	fixed4 fragCombine (v2f i) : SV_Target
	{
		fixed4 bgColor = tex2D(_MainTex, i.uv);
		fixed4 lightColor = tex2D(_LightTex, i.uv) * _SunColor * _Intensity * _GlobalIntensity;
		
		half4 patternColor = getPatternValue(i.uv);
		half3 mergedColor = clamp(lightColor.xyz - ((1.0f - patternColor.xyz) * _PatternIntensity), 0, 20);

		fixed4 finalLightColor = fixed4(mergedColor.xyz, 1.0f);
		
		#if BLEND_MODE_ADDITIVE
		return bgColor + saturate(finalLightColor);
		#else
		return screenBlend(bgColor, saturate(finalLightColor));
		#endif
	}

	fixed4 fragLightMask (v2f i) : SV_Target
	{
		fixed4 bgColor = tex2D(_MainTex, i.uv);
		fixed4 skyColor = tex2D(_SkyboxTex, i.uv);

		fixed2 distOffset = _SunViewSpace.xy - i.uv;
		fixed weight = saturate(_SunShaftRadius - length(distOffset));
		
		fixed3 deltaColor = abs(skyColor.rgb - bgColor.rgb);
		fixed3 finalColor = fixed3(0, 0, 0);

		// TODO - remove conditional
		if (Luminance(deltaColor) < .2f) {
			finalColor = transformColor(skyColor) * weight;
		}

		return fixed4(finalColor, 1);
	}

	v2fBlur vertBlur(appdata_img v) {
		v2fBlur o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		
		o.uv.xy =  v.texcoord.xy;
		o.blurVector = (_SunViewSpace.xy - v.texcoord.xy) * _BlurStepRadius.xy;	
		
		return o; 
	}

	half4 fragBlur(v2fBlur i) : SV_Target 
	{	
		half4 color = half4(0, 0, 0, 0);
		for(int j = 0; j < BLUR_PASSES; j++)   
		{	
			half4 tmpColor = tex2D(_MainTex, i.uv);
			color += tmpColor;
			i.uv.xy += i.blurVector; 	
		}
		return color / (float)BLUR_PASSES;
	}	

	ENDCG

	SubShader
	{
		Pass
		{
			Cull Off ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragCombine
			
			ENDCG
		}

		Pass
		{
			Cull Off ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragLightMask
			
			ENDCG
		}

		Pass
		{ 
			Cull Off ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vertBlur
			#pragma fragment fragBlur
			
			ENDCG
		}
	}

	Fallback off
}
