Shader "ConcussionVR/CalmSky"
{
    Properties
    {
        _HorizonColor ("Horizon", Color) = (0.48,0.72,0.76,1)
        _ZenithColor ("Zenith", Color) = (0.19,0.48,0.65,1)
        _DawnColor ("Dawn", Color) = (1.0,0.76,0.48,1)
    }
    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" }
        Cull Front
        ZWrite Off
        Fog { Mode Off }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            fixed4 _HorizonColor;
            fixed4 _ZenithColor;
            fixed4 _DawnColor;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 viewDirection : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDirection = worldPosition - _WorldSpaceCameraPos.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half3 direction = normalize(i.viewDirection);
                half heightBlend = smoothstep(0.18, 0.82, saturate(direction.y * 0.72 + 0.42));
                half3 color = lerp(_HorizonColor.rgb, _ZenithColor.rgb, heightBlend);
                half3 dawnDirection = normalize(half3(-0.20, 0.20, 0.96));
                half dawnGlow = pow(saturate(dot(direction, dawnDirection)), 24) * (1 - heightBlend) * 0.32;
                color = lerp(color, _DawnColor.rgb, dawnGlow);
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
