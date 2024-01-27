struct OutlineAppData
{
  float4 vertex : POSITION;
  fixed4 normal : NORMAL;
};

struct OutlineV2F
{
  float4 pos : SV_POSITION;
  float4 worldPos : TEXCOORD0;
};

float4 _OutlineColor;
float _OutlineThickness;
float _OutlineMaxThickness;

OutlineV2F OutlineVert (OutlineAppData v)
{
  float3 worldNormal = mul(unity_ObjectToWorld, fixed4(v.normal.xyz, 0));
  float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
  float camDist = length(_WorldSpaceCameraPos - worldPos);
  float thickness = min(_OutlineMaxThickness, _OutlineThickness * camDist);
  worldPos.xyz += normalize(worldNormal) * thickness;

  OutlineV2F o;
  o.pos = UnityWorldToClipPos(worldPos);
  o.worldPos = worldPos;

  return o;
}

fixed4 OutlineFrag (OutlineV2F i) : SV_Target
{
  // Get base diffuse color
  fixed3 diffuse = _OutlineColor.rgb;

  return fixed4(diffuse, 1);
}