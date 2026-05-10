Shader "UI/CrosshairThick"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Thickness ("Thickness", Range(1, 5)) = 1.5
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _Thickness;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texel = _MainTex_TexelSize.xy * _Thickness;

                // Сэмплируем соседние пиксели и берём максимальную альфу
                fixed4 col = tex2D(_MainTex, uv);
                col.a = max(col.a, tex2D(_MainTex, uv + float2(texel.x, 0)).a);
                col.a = max(col.a, tex2D(_MainTex, uv - float2(texel.x, 0)).a);
                col.a = max(col.a, tex2D(_MainTex, uv + float2(0, texel.y)).a);
                col.a = max(col.a, tex2D(_MainTex, uv - float2(0, texel.y)).a);

                col.rgb = _Color.rgb;
                col.a *= _Color.a;
                return col;
            }
            ENDCG
        }
    }
}