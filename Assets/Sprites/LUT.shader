Shader "Hidden/LUT"
{
    Properties
    {
      _MainTex("Texture", 2D) = "white" {}
      _Color("Color", Color) = (1, 1, 1, 1)
        
      _LUT_IN ("Texture", 2D) = "white" {} 
      _LUT_OUT_PRERIPPLE("Texture", 2D) = "white" {}
      _LUT_OUT_POSTRIPPLE("Texture", 2D) = "white" {}

      _PlayerPosition("PlayerPosition", Vector) = (0.5,0.5,0.5,1)
      _AnimationTime("AnimationTime", Float) = 0
      _AnimationLength("AnimationLength", Float) = 0
          _Annulus("Annulus Radius", Float) = 1
        _MaxRange("Outer Radius", Float) = 1
        _DistortionStrength("DistortionStrength", Float) = 1
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

            float4 _PlayerPosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);          
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            fixed4 _Color;

            float _AnimationTime;
            float _AnimationLength;

            half _Annulus;
            half _MaxRange;
            half _DistortionStrength;

            sampler2D _LUT_IN;
            sampler2D _LUT_OUT_PRERIPPLE;
            sampler2D _LUT_OUT_POSTRIPPLE;
            float4 _LUT_IN_TexelSize;
            float4 _LUT_OUT_PRERIPPLE_TexelSize;
            float4 _LUT_OUT_POSTRIPPLE_TexelSize;

            float4 SwitchColorPost(float4 col) { 
                int size = _LUT_IN_TexelSize.z;
                [loop]
                for (int i = 0; i < size; ++i) {
                  float4 c = tex2D(_LUT_IN, i * _LUT_IN_TexelSize.x);
                  if (col.x == c.x && col.y == c.y && col.z == c.z) {
                    return tex2D(_LUT_OUT_POSTRIPPLE, i * _LUT_OUT_POSTRIPPLE_TexelSize.x);
                  }
                }
                return col;
            }

            float4 SwitchColorPre(float4 col) { 
                int size = _LUT_IN_TexelSize.z;
                [loop]
                for (int i = 0; i < size; ++i) {
                  float4 c = tex2D(_LUT_IN, i * _LUT_IN_TexelSize.x);
                  if (col.x == c.x && col.y == c.y && col.z == c.z) {
                    return tex2D(_LUT_OUT_PRERIPPLE, i * _LUT_OUT_PRERIPPLE_TexelSize.x);
                  }
                }
                return col;
            }

            fixed4 frag(v2f i) : SV_Target
            {
              half percent = _AnimationTime / _AnimationLength;
              percent = percent + 0.2;
              percent = (percent*percent*percent);

               half dist = distance(i.uv, _PlayerPosition);
               _Annulus = sqrt(percent * _Annulus);
               //_Annulus = _Annulus * _Annulus;
               _MaxRange = percent + _Annulus;
               dist = saturate((dist - _MaxRange + _Annulus) / (_Annulus)); //interpolation value with zero as the inside edge of the annulus and 1 as the outside edge

                if (dist > 0 && dist < 1)
                {
                    dist = dist * dist;
                   // float4 col = tex2D(_MainTex, (i.uv + (0.01 * dist * normalize( _PlayerPosition - i.uv)) * _DistortionStrength)); //our uv, but shifted outwards (in local space)
                    float2 uv =  (i.uv + (0.01 * dist * normalize( _PlayerPosition - i.uv)) * _DistortionStrength);
                    float4 col = tex2D(_MainTex, uv);
                    col = SwitchColorPost(col);
                    return col;
                }
                else if (dist > 0)
                {
                    float4 col = tex2D(_MainTex, i.uv);
                  return SwitchColorPre(col);
                }
                else {
                    float4 col = tex2D(_MainTex, i.uv);
                    return SwitchColorPost(col);
                }


                
            }
            ENDCG
        }
    }
}
