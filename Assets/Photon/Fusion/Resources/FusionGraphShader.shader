Shader "Fusion Graph Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GoodColor("Good Color", Color) = (0,1,0,1)
        _BadColor("Bad Color", Color) = (1,0,0,1)
        _FlagColor("Flag Color", Color) = (1,1,0,1)
        _NoneColor("Flag Color", Color) = (0,0,0,0)

        _BarsWidth("Bars Width", Range(0.0, 1.0)) = 0.1// 0.5
        _CapsHeight("End Caps Size", Range(0.0, 5)) = 1.5
        
		[MaterialToggle] EndCap("End Cap", Float) = 0 //1
		[MaterialToggle] AnimateInOut("Animate In/Out", Float) = 0//1
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        
        Cull Off
        ZTest Off
        ZWrite Off
        Lighting Off
        
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile _ ENDCAP_ON
            #pragma multi_compile _ ANIMATEINOUT_ON

            #include "UnityCG.cginc"

            struct vertdata_t {
                float4 v : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct fragdata_t {
                float4 v : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            #define ANIM_DURATION 0.05
            
            // this needs to be here or UGUI system complains
            sampler2D _MainTex;

            float _BarsWidth;
            float _CapsHeight;
            
            fixed4 _GoodColor;
            fixed4 _BadColor;
            fixed4 _FlagColor;
            fixed4 _NoneColor;

            // data for bar graph
            uniform float _Data[1024];
            uniform float _Intensity[1024];
            uniform float _Count;
            uniform float _Height; 
             
            fragdata_t vert(const vertdata_t v) {
                fragdata_t f;
                f.v = UnityObjectToClipPos(v.v);
                f.uv = v.uv;
                return f;
            }

            // Smooth HSV to RGB conversion
            // https://www.shadertoy.com/view/MsS3Wc
            // 
            // The MIT License
            // Copyright © 2014 Inigo Quilez
            // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            // 
            // Converting from HSV to RGB leads to C1 discontinuities, for the RGB components
            // are driven by picewise linear segments. Using a cubic smoother (smoothstep) makes 
            // the color transitions in RGB C1 continuous when linearly interpolating the hue H.
            // 
            // C2 continuity can be achieved as well by replacing smoothstep with a quintic
            // polynomial. Of course all these cubic, quintic and trigonometric variations break 
            // the standard (http://en.wikipedia.org/wiki/HSL_and_HSV), but they look better.
            float3 HSV2RGB(float3 c) {
                float3 rgb = clamp( abs(fmod(c.x*6.0+float3(0.0,4.0,2.0),6)-3.0)-1.0, 0, 1);
                rgb = rgb*rgb*(3.0-2.0*rgb);
                return c.z * lerp( float3(1,1,1), rgb, c.y);
            }

            // HSV => RGB and RGB => HSV algorithm from here: https://www.programmersought.com/article/59764720552/
            float3 RGB2HSV(float3 c) {
                const float4 k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	            const float4 p = lerp(float4(c.bg, k.wz), float4(c.gb, k.xy), step(c.b, c.g));
	            const float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
	            const float d = q.x - min(q.w, q.y);
	            const float e = 1.0e-10;
	            return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            fixed4 frag(const fragdata_t f) : SV_Target {
                const float i = floor(f.uv.x * _Count);
                //const float i_next = floor((f.uv.x + ((1.0/_Count) * _BarsWidth)) * _Count);
                const float r = 1;

                #if ANIMATEINOUT_ON
                const float m = i / _Count;
                const float ai = saturate(pow(1 - max(0, (m - (1 - ANIM_DURATION)) / ANIM_DURATION), 2));
                const float ao = saturate(pow(min(1, m / ANIM_DURATION), 2));
                #else
                const float ai = 1;
                const float ao = 1;
                #endif

                const float rv = max(_Data[i], _CapsHeight / _Height);
                const float fv = _Intensity[i];

                #ifdef ENDCAP_ON
                const float v = rv * ai * ao;
                const float b = (v - f.uv.y) >= (_CapsHeight / _Height);
                #else
                const float v = max(_Data[i], 1 / _Count) * ai * ao;
                const float b = 0;
                #endif
                
                const float a = !(f.uv.y > v);
                const float ds = (ai - 0.5) + (ao - 0.5);

                // calculate alpha and rgb
                float4 c;
                 //c.rgb = lerp(_BadColor.rgb, _GoodColor.rgb, rv) * c.a;
                if (_Count == 0) {
                    c.a = 0.1;
                    c.rgb = _NoneColor.rgb;
                }
                else {
                    c.a = (((0.25 * b) * a) + (a * !b)) * r * a * ai * ao;
                    c.rgb = lerp(_GoodColor.rgb, _BadColor.rgb, fv) * c.a;
                }
                // apply desaturation
                float3 hsv = RGB2HSV(c.rgb);
                hsv.z *= ds;
                c.rgb = HSV2RGB(hsv);
                
                return c;
            }
            
            ENDCG
        }
    }
}
