Shader "ConcussionVR/ClinicalSurface"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.35
        _Metallic ("Metallic", Range(0,1)) = 0
        _PebbleDensity ("Pebble Density", Range(0,96)) = 0
        _PebbleStrength ("Pebble Strength", Range(0,0.3)) = 0
        _EmissionColor ("Emission", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 250
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            fixed4 _Color;
            half _Smoothness;
            half _Metallic;
            half _PebbleDensity;
            half _PebbleStrength;
            fixed4 _EmissionColor;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                half3 worldNormal : TEXCOORD1;
                half3 ambient : TEXCOORD2;
                float2 uv : TEXCOORD3;
                SHADOW_COORDS(4)
                UNITY_FOG_COORDS(5)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.ambient = ShadeSH9(half4(normalize(o.worldNormal), 1));
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half3 n = normalize(i.worldNormal);
                half2 pebbleCell = frac(i.uv * half2(_PebbleDensity * 2.0, _PebbleDensity)) - 0.5;
                half pebble = smoothstep(0.44, 0.10, length(pebbleCell));
                half pebbleHeight = pebble * _PebbleStrength;
                float3 positionDx = ddx(i.worldPos);
                float3 positionDy = ddy(i.worldPos);
                float2 uvDx = ddx(i.uv);
                float2 uvDy = ddy(i.uv);
                float3 perpendicularY = cross(positionDy, n);
                float3 perpendicularX = cross(n, positionDx);
                float3 tangent = perpendicularY * uvDx.x + perpendicularX * uvDy.x;
                float3 bitangent = perpendicularY * uvDx.y + perpendicularX * uvDy.y;
                float tangentScale = rsqrt(max(dot(tangent, tangent), dot(bitangent, bitangent)) + 0.0001);
                float3 pebbleGradient = ddx(pebbleHeight) * tangent + ddy(pebbleHeight) * bitangent;
                n = normalize(n - pebbleGradient * tangentScale);
                half3 l = normalize(_WorldSpaceLightPos0.xyz);
                half3 v = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                half3 h = normalize(l + v);
                half attenuation = SHADOW_ATTENUATION(i);
                half diffuse = saturate(dot(n, l));
                half specular = pow(saturate(dot(n, h)), lerp(8, 96, _Smoothness));
                half3 dielectric = lerp(0.035.xxx, _Color.rgb, _Metallic);
                half grain = frac(sin(dot(floor(i.worldPos * 7.5), float3(12.9898, 78.233, 37.719))) * 43758.5453);
                half surfaceVariation = lerp(0.965, 1.035, grain);
                surfaceVariation *= lerp(1.0, lerp(0.88, 1.06, pebble), saturate(_PebbleStrength * 3.5));
                half3 lit = _Color.rgb * surfaceVariation * (i.ambient + _LightColor0.rgb * diffuse * attenuation);
                lit += dielectric * specular * _Smoothness * attenuation;
                lit += _EmissionColor.rgb;
                fixed4 output = fixed4(lit, _Color.a);
                UNITY_APPLY_FOG(i.fogCoord, output);
                return output;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
