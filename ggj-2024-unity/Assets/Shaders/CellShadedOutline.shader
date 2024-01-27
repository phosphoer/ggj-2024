Shader "Custom/CellShadedOutline"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _OutlineColor ("Outline Color", Color) = (1,1,1,1)
    _OutlineThickness ("Outline Thickness", float) = 0.1
    _OutlineMaxThickness ("Outline Max Thickness", float) = 0.1
    _MainTex ("Texture", 2D) = "white" {}
    _LightRamp ("Light Ramp", 2D) = "white" {}
    _VertexColorWeight ("Vertex Color Weight", float) = 1

    [Enum(Off,0,On,1)] 
    _ZWrite ("ZWrite", Float) = 1
    
    [Enum(Always, 0, Less, 2, Equal, 3, LEqual, 4, GEqual, 5)] 
    _ZTest ("ZTest", Float) = 4

    [Enum(Off,0,On,1)] 
    _OutlineZWrite ("ZWrite", Float) = 1
    
    [Enum(Always, 0, Less, 2, Equal, 3, LEqual, 4, GEqual, 5)] 
    _OutlineZTest ("ZTest", Float) = 4
  }
  SubShader
  {
    CGINCLUDE 
      #include "UnityCG.cginc"
      #include "CellShading.cginc"
      #include "Outline.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        fixed4 normal : NORMAL;
        fixed4 color : COLOR;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 pos : SV_POSITION;
        fixed4 color : COLOR;
        float2 uv : TEXCOORD0;
        SHADOW_COORDS(1)
        fixed3 worldNormal : TEXCOORD2;
        float4 worldPos : TEXCOORD3;
      };

      sampler2D _MainTex;
      float4 _Color;
      float _VertexColorWeight;

      v2f vert (appdata v)
      {
        v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityWorldToClipPos(o.worldPos);
        o.uv = v.uv;
        o.worldNormal = mul(unity_ObjectToWorld, fixed4(v.normal.xyz, 0));
        o.color = v.color;

        TRANSFER_SHADOW(o)

        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        // Get base diffuse color
        fixed3 diffuse = _Color.rgb * tex2D(_MainTex, i.uv).rgb * lerp(fixed3(1, 1, 1), i.color.rgb, _VertexColorWeight);

        fixed lightAtten = 1;
        #ifdef POINT
        unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(i.worldPos, 1)).xyz;
        fixed shadow = UNITY_SHADOW_ATTENUATION(i, i.worldPos);
        lightAtten = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r * shadow;
        #endif

        diffuse *= CalculateLighting(normalize(i.worldNormal), lightAtten, SHADOW_ATTENUATION(i)).rgb;
        UNITY_APPLY_FOG(i.fogCoord, diffuse);

        return fixed4(diffuse, 1);
      }
    ENDCG 

    Pass
    {
      Tags { "RenderType"="Opaque" }

      ZWrite [_ZWrite]
      ZTest [_ZTest]

      Stencil 
      {
        Ref 2
        Comp always
        Pass replace
      }

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
    }

    Pass
    {
      Tags { "RenderType"="Opaque" }

      ZWrite [_OutlineZWrite]
      ZTest [_OutlineZTest]

      Stencil 
      {
        Ref 2
        Comp notequal
        Pass keep
      }

      CGPROGRAM
      #pragma vertex OutlineVert
      #pragma fragment OutlineFrag
      ENDCG
    }
  }

  Fallback "Diffuse"
}
