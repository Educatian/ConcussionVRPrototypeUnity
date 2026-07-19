Shader "ConcussionVR/GuideGlow"
{
    Properties
    {
        _Color ("Color", Color) = (0.12,0.9,0.82,0.92)
        _EmissionColor ("Emission", Color) = (0.05,0.7,0.62,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            fixed4 _EmissionColor;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; half3 normal : TEXCOORD0; half3 viewDir : TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                half rim = pow(1 - saturate(dot(normalize(i.normal), normalize(i.viewDir))), 2.2);
                half3 color = _Color.rgb + _EmissionColor.rgb * (0.3 + rim * 0.8);
                return fixed4(color, saturate(_Color.a * (0.82 + rim * 0.18)));
            }
            ENDCG
        }
    }
}
