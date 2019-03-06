Shader "MyShader/HoloShader"
{
    Properties {
	  _texture ("Diffuse Texture", 2D) = "white" {}
      _RimColor ("Rim Color", Color) = (0,0.5,0.5,0.0)
      _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
	  _rimCutOff ("Rim cutoff", Range(0.2,0.9)) = 0.2
    }
    SubShader {
	  Tags{"Queue" = "Transparent"}

	  Pass{
	  	  Zwrite On
		  ColorMask 0
	  }

      CGPROGRAM
      #pragma surface surf Lambert alpha:fade
      struct Input {
          float3 viewDir;
		  float2 uv_texture;
      };

      float4 _RimColor;
      float _RimPower;
	  half _rimCutOff;
	  sampler2D _texture;

      void surf (Input IN, inout SurfaceOutput o) {
		  o.Albedo = tex2D(_texture, IN.uv_texture).rgb;
          half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
          o.Emission = rim > _rimCutOff ? _RimColor.rgb * pow (rim, _RimPower) : 0 ;
		  o.Alpha =  pow (rim, _RimPower);
      }
      ENDCG
    } 
    Fallback "Diffuse"
}
