Shader "Custom/Blinn-Phong"
{
     Properties
     {
         _Diffuse ("Diffuse Color", Color) = (1,1,1,1)
         _Specular ("Specular Color", Color) = (1,1,1,1)
         _Gloss ("Gloss", Range(8, 256)) = 8
     }
     SubShader
     {
         Tags { "RenderType"="Opaque" }
         LOD 100
 
         Pass
         {
             Tags { "LightMode"="ForwardBase" }
 
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
 
             #include "UnityCG.cginc"
             #include "Lighting.cginc"
 
             struct appdata
             {
                 float4 vertex : POSITION;
                 float3 normal : NORMAL;
             };
 
             struct v2f
             {
                 float4 vertex : SV_POSITION;
                 float3 worldPos : TEXCOORD0;
                 float3 worldNormal : TEXCOORD1;
             };
 
             fixed4 _Diffuse;
             fixed4 _Specular;
             float _Gloss;
                         
             v2f vert (appdata v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.worldNormal = UnityObjectToWorldNormal(v.normal);
                 o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                 return o;
             }
             
             fixed4 frag (v2f i) : SV_Target
             {
                 // specular
                 float3 worldNormal = normalize(i.worldNormal);
                 float3 lightDir = UnityWorldSpaceLightDir(i.worldPos);
                 lightDir = normalize(lightDir);
                 float3 viewDir = UnityWorldSpaceViewDir(i.worldPos);
                 viewDir = normalize(viewDir);
                 float3 halfDir = normalize(lightDir + viewDir);
                 float d = max(0, dot(halfDir, worldNormal));
                 float3 spec = _LightColor0.rgb * _Specular.rgb * pow(d, _Gloss);
 
                 // diffuse
                 float3 diff = _LightColor0.rgb * _Diffuse.rgb * max(0, dot(lightDir, worldNormal));
 
                 float3 c = spec + diff + UNITY_LIGHTMODEL_AMBIENT.rgb;
                 return fixed4(c, 1);
             }
             ENDCG
         }
     }
 }