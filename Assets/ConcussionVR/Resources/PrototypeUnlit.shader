Shader "ConcussionVR/PrototypeUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct AppData
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
            };

            fixed4 _Color;

            Varyings Vert(AppData input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
