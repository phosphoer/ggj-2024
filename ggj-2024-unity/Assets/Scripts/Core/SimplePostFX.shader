Shader "Custom/SimplePostFX" 
{
  Properties 
  {
    _MainTex ("Texture", 2D) = "white" {}
    _SourceTex ("Source Texture", 2D) = "white" {}
    _BloomParams ("Bloom Params", Vector) = (1, 1, 1, 0)
    _ColorParams ("Color Params", Vector) = (1, 1, 0, 0)
    _ChannelMixerRed ("Channel Mixer Red", Vector) = (1, 0, 0, 0)
    _ChannelMixerGreen ("Channel Mixer Green", Vector) = (0, 1, 0, 0)
    _ChannelMixerBlue ("Channel Mixer Blue", Vector) = (0, 0, 1, 0)
  }

  CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _SourceTex;
    float4 _MainTex_TexelSize;

    // R = Filter Size, G = Threshold, B = Intensity, A = Soft Threshold
    float4 _BloomParams;

    // R = Saturation, G = Contrast, B = Temperature, A = Tint
    float4 _ColorParams;

    // Channel mixers
    float4 _ChannelMixerRed;
    float4 _ChannelMixerGreen;
    float4 _ChannelMixerBlue;

    struct VertexData 
    {
      float4 vertex : POSITION;
      float2 uv : TEXCOORD0;
    };

    struct Interpolators 
    {
      float4 pos : SV_POSITION;
      float2 uv : TEXCOORD0;
    };

    half3 Prefilter (half3 c)
    {
      half brightness = max(c.r, max(c.g, c.b));
      half knee = _BloomParams.g * _BloomParams.a;
      half soft = brightness - _BloomParams.g + knee;
      soft = clamp(soft, 0, 2 * knee);
      soft = soft * soft / (4 * knee + 0.00001);
      half contribution = max(soft, brightness - _BloomParams.g);
      contribution /= max(brightness, 0.00001);
      return c * contribution;
    }

    half3 Sample (float2 uv) 
    {
			return tex2D(_MainTex, uv).rgb;
		}

    half3 SampleBox (float2 uv, float delta) 
    {
			float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
			half3 s =
				Sample(uv + o.xy) + Sample(uv + o.zy) +
				Sample(uv + o.xw) + Sample(uv + o.zw);
			return s * 0.25f;
		}

    half3 Saturation (half3 c, float saturationDelta)
    {
      const float3 kLuminance = float3(0.3086, 0.6094, 0.0820);
      half3 intensity = dot(c, kLuminance);
      return lerp(intensity, c, saturationDelta);
    }

    half3 Contrast (half3 c, float contrast)
    {
      const float kContrastMidpoint = 0.21763; // 0.5^2.2
      return (c - kContrastMidpoint) * contrast + kContrastMidpoint;
    }

    half3 ChannelMixer (half3 c, half3 channelRed, half3 channelGreen, half3 channelBlue)
    {
      return half3(dot(c, channelRed), dot(c, channelGreen), dot(c, channelBlue));
    }

    // From https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/White-Balance-Node.html
    half3 WhiteBalance (half3 c, float temperature, float tint)
    {
      // Range ~[-1.67;1.67] works best
      float t1 = temperature * 10 / 6;
      float t2 = tint * 10 / 6;

      // Get the CIE xy chromaticity of the reference white point.
      // Note: 0.31271 = x value on the D65 white point
      float x = 0.31271 - t1 * (t1 < 0 ? 0.1 : 0.05);
      float standardIlluminantY = 2.87 * x - 3 * x * x - 0.27509507;
      float y = standardIlluminantY + t2 * 0.05;

      // Calculate the coefficients in the LMS space.
      const half3 w1 = half3(0.949237, 1.03542, 1.08728); // D65 white point

      // CIExyToLMS
      float Y = 1;
      float X = Y * x / y;
      float Z = Y * (1 - x - y) / y;
      float L = 0.7328 * X + 0.4296 * Y - 0.1624 * Z;
      float M = -0.7036 * X + 1.6975 * Y + 0.0061 * Z;
      float S = 0.0030 * X + 0.0136 * Y + 0.9834 * Z;
      half3 w2 = half3(L, M, S);

      half3 balance = half3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);

      const half3x3 LIN_2_LMS_MAT = {
          3.90405e-1, 5.49941e-1, 8.92632e-3,
          7.08416e-2, 9.63172e-1, 1.35775e-3,
          2.31082e-2, 1.28021e-1, 9.36245e-1
      };

      const half3x3 LMS_2_LIN_MAT = {
          2.85847e+0, -1.62879e+0, -2.48910e-2,
          -2.10182e-1,  1.15820e+0,  3.24281e-4,
          -4.18120e-2, -1.18169e-1,  1.06867e+0
      };

      half3 lms = mul(LIN_2_LMS_MAT, c);
      lms *= balance;
      return mul(LMS_2_LIN_MAT, lms);
    }

    Interpolators VertexProgram (VertexData v) 
    {
      Interpolators i;
      i.pos = UnityObjectToClipPos(v.vertex);
      i.uv = v.uv;
      return i;
    }
  ENDCG

  SubShader 
  {
    Cull Off
    ZTest Always
    ZWrite Off

    // Downsample Prefilter
    Pass 
    {
      CGPROGRAM
        #pragma vertex VertexProgram
        #pragma fragment FragmentProgram

        half4 FragmentProgram (Interpolators i) : SV_Target 
        {
          return half4(Prefilter(SampleBox(i.uv, _BloomParams.r)), 1);
        }
      ENDCG
    }

    // Downsample
    Pass 
    {
      CGPROGRAM
        #pragma vertex VertexProgram
        #pragma fragment FragmentProgram

        half4 FragmentProgram (Interpolators i) : SV_Target 
        {
          return half4(SampleBox(i.uv, _BloomParams.r), 1);
        }
      ENDCG
    }

    // Upsample
    Pass 
    {
      Blend One One 

      CGPROGRAM
        #pragma vertex VertexProgram
        #pragma fragment FragmentProgram

        half4 FragmentProgram (Interpolators i) : SV_Target 
        {
          return saturate(half4(_BloomParams.b * SampleBox(i.uv, _BloomParams.r * 0.5), 1));
        }
      ENDCG
    }

    // Final sample
    Pass 
    {
      CGPROGRAM
        #pragma vertex VertexProgram
        #pragma fragment FragmentProgram

        half4 FragmentProgram (Interpolators i) : SV_Target 
        {
          half4 c = tex2D(_SourceTex, i.uv);
          c.rgb += _BloomParams.b * SampleBox(i.uv, _BloomParams.r * 0.5);
          c.rgb = Saturation(c.rgb, _ColorParams.r);
          c.rgb = Contrast(c.rgb, _ColorParams.g);
          c.rgb = ChannelMixer(c.rgb, _ChannelMixerRed, _ChannelMixerGreen, _ChannelMixerBlue);
          c.rgb = WhiteBalance(c.rgb, _ColorParams.b, _ColorParams.a);
          return saturate(c);
        }
      ENDCG
    }
  }
}