Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                float2 up = float2(0, _MainTex_TexelSize.y) * _OutlineWidth;
                float2 down = float2(0, -_MainTex_TexelSize.y) * _OutlineWidth;
                float2 right = float2(_MainTex_TexelSize.x, 0) * _OutlineWidth;
                float2 left = float2(-_MainTex_TexelSize.x, 0) * _OutlineWidth;

                fixed4 cUp = tex2D(_MainTex, IN.texcoord + up);
                fixed4 cDown = tex2D(_MainTex, IN.texcoord + down);
                fixed4 cRight = tex2D(_MainTex, IN.texcoord + right);
                fixed4 cLeft = tex2D(_MainTex, IN.texcoord + left);

                float outlineAlpha = max(max(cUp.a, cDown.a), max(cRight.a, cLeft.a));
                outlineAlpha = clamp(outlineAlpha - c.a, 0, 1);

                fixed4 finalColor = lerp(c, _OutlineColor, outlineAlpha);
                finalColor.a = c.a + outlineAlpha * _OutlineColor.a;

                return finalColor;
            }
            ENDCG
        }
    }
}