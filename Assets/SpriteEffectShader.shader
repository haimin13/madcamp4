Shader "Custom/SpriteEffectShader"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Character Sprite", 2D) = "white" {} 
        _EffectTex ("Wave Texture", 2D) = "white" {}
        _EffectColor ("Effect Color", Color) = (1, 0.5, 0, 1)
        _ScrollY ("Vertical Scroll", Range(-1, 1)) = 0.0
        _Alpha ("Effect Alpha", Range(0, 1)) = 1.0
        _Intensity ("Effect Intensity", Range(1, 5)) = 2.0 // 효과 강도 조절 변수 추가
        
        // --- 피격 효과를 위한 변수 (수정) ---
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1) // 피격 시 번쩍일 색상 (기본값: 흰색)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0.0      // 피격 효과의 강도 (0: 꺼짐, 1: 최대)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv_main : TEXCOORD0;
                float2 uv_effect : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _EffectTex;
            float4 _EffectTex_ST;
            float4 _EffectColor;
            float _ScrollY;
            float _Alpha;
            float _Intensity;
            
            // --- 피격 효과 변수 선언 ---
            float4 _FlashColor;
            float _FlashAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_main = v.uv;
                o.uv_effect = TRANSFORM_TEX(v.uv, _EffectTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 charColor = tex2D(_MainTex, i.uv_main);

                if (charColor.a < 0.1) {
                    discard;
                }

                float2 effectUV = i.uv_effect;
                effectUV.y += _ScrollY;

                fixed waveValue = tex2D(_EffectTex, effectUV).r;

                // 1. 효과 색상을 계산합니다.
                fixed3 effectRgb = _EffectColor.rgb * waveValue * _Alpha * _Intensity;
                
                // 2. 원래 캐릭터 색상에 효과 색상을 더합니다.
                fixed3 finalRgb = charColor.rgb + effectRgb;

                // 3. 피격 효과를 덧입힙니다.
                //    _FlashAmount 값에 따라 최종 색상과 피격 색상을 섞습니다.
                finalRgb = lerp(finalRgb, _FlashColor.rgb, _FlashAmount);
                
                // 4. 계산된 RGB 색상과 원래의 투명도를 합쳐 반환합니다.
                return fixed4(finalRgb, charColor.a);
            }
            ENDCG
        }
    }
}
