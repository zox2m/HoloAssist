// Copyright (C) 2020 VIRNECT CO., LTD.
// All rights reserved.

Shader "VIRNECT/BackgroundShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _UvTopLeftRight("UV of top corners", Vector) = (0, 1, 1, 1)
        _UvBottomLeftRight("UV of bottom corners", Vector) = (0, 0, 1, 0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

            Pass
        {
            CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

            uniform float4 _UvTopLeftRight;
            uniform float4 _UvBottomLeftRight;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                float2 uvTop = lerp(_UvTopLeftRight.xy, _UvTopLeftRight.zw, v.uv.x);
                float2 uvBottom = lerp(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, v.uv.x);

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = lerp(uvTop, uvBottom, v.uv.y);

                // Instant preview's texture is transformed differently.
                //o.uv = o.uv.yx;
               // o.uv.x = 1.0 - o.uv.x;
                return o;
            }

            sampler2D _MainTex;

            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}