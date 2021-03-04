Shader "Hidden/LUT"
{
    Properties
    {
      _MainTex("Texture", 2D) = "white" {}
      _Color("Color", Color) = (1, 1, 1, 1)
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

            fixed4 frag(v2f i) : SV_Target
            {
                half percent = _AnimationTime / _AnimationLength;

               half dist = distance(i.uv, _PlayerPosition);
               _Annulus = sqrt(percent * _Annulus);
               //_Annulus = _Annulus * _Annulus;
               _MaxRange = percent + _Annulus;
               dist = saturate((dist - _MaxRange + _Annulus) / (_Annulus)); //interpolation value with zero as the inside edge of the annulus and 1 as the outside edge

                if (dist > 0 && dist < 1)
                {
                    dist = dist * dist;
                    float4 col = tex2D(_MainTex, (i.uv + (0.01 * dist * normalize(i.uv - _PlayerPosition)) * _DistortionStrength)); //our uv, but shifted outwards (in local space)

                    if (dist < 0.5) {
                        fixed4 white = fixed4(1, 1, 1, 1);
                        if (distance(col, white) < 0.5)
                            col = _Color;
                        return col;
                    }
                    else {
                        return col;
                    }
                }
                else if (dist > 0)
                {
                    return tex2D(_MainTex, i.uv);
                }
                else {
                    float4 col = tex2D(_MainTex, i.uv);
                    fixed4 white = fixed4(1, 1, 1, 1);
                    if (distance(col, white) < 0.5)
                        col = _Color;
                    return col;
                }


                
            }
            ENDCG
        }
    }
}
