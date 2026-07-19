Shader "ConcussionVR/CalmWater"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.035,0.22,0.29,1)
        _ShallowColor ("Shallow Color", Color) = (0.16,0.52,0.58,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.88
        _WaveAmount ("Wave Amount", Range(0,0.04)) = 0.008
        _NormalStrength ("Normal Strength", Range(0,3)) = 1.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+5" }
        LOD 250
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            fixed4 _DeepColor;
            fixed4 _ShallowColor;
            half _Smoothness;
            half _WaveAmount;
            half _NormalStrength;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                half3 worldNormal : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            v2f vert(appdata v)
            {
                float phaseA = v.vertex.x * 0.22 + v.vertex.z * 0.16 + _Time.y * 0.18;
                float phaseB = v.vertex.x * -0.15 + v.vertex.z * 0.26 + _Time.y * 0.12 + 1.7;
                v.vertex.y += (sin(phaseA) + sin(phaseB) * 0.55) * _WaveAmount;
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float phaseA = i.worldPos.x * 0.22 + i.worldPos.z * 0.16 + _Time.y * 0.18;
                float phaseB = i.worldPos.x * -0.15 + i.worldPos.z * 0.26 + _Time.y * 0.12 + 1.7;
                half slopeX = (cos(phaseA) * 0.22 - cos(phaseB) * 0.0825) * _WaveAmount * _NormalStrength * 28;
                half slopeZ = (cos(phaseA) * 0.16 + cos(phaseB) * 0.143) * _WaveAmount * _NormalStrength * 28;
                half3 n = normalize(i.worldNormal + half3(-slopeX, 0, -slopeZ));
                half horizonBlend = pow(1 - saturate(dot(n, half3(0, 1, 0))), 2);
                half3 v = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                half fresnel = pow(1 - saturate(dot(n, v)), 3);
                half shimmer = sin(i.worldPos.x * 0.42 + i.worldPos.z * 0.31) * sin(i.worldPos.x * -0.27 + i.worldPos.z * 0.48) * 0.5 + 0.5;
                half3 color = lerp(_DeepColor.rgb, _ShallowColor.rgb, saturate(fresnel * 0.70 + shimmer * 0.055 + horizonBlend * 0.08));
                half3 h = normalize(normalize(_WorldSpaceLightPos0.xyz) + v);
                color += _LightColor0.rgb * pow(saturate(dot(n, h)), 72) * _Smoothness * 0.35;
                fixed4 output = fixed4(color, 1);
                UNITY_APPLY_FOG(i.fogCoord, output);
                return output;
            }
            ENDCG
        }
    }
}
