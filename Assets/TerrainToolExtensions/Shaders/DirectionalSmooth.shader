    Shader "TerrainTools/DirectionalSmooth" {

    Properties { _MainTex ("Texture", any) = "" {} }

    SubShader {

        ZTest Always Cull Off ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height

            sampler2D _BrushTex;

            float4 _BrushParams;
            #define BRUSH_STRENGTH      (_BrushParams[0])
            #define BRUSH_TARGETHEIGHT  (_BrushParams[1])
			#define BRUSH_MODE			(_BrushParams[2])

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

        ENDCG


        Pass
        {
            Name "Directional Smooth Height"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment DirectionalSmoothHeight

            float4 DirectionalSmoothHeight(v2f i) : SV_Target
            {
                float2 brushUV = PaintContextUVToBrushUV(i.uv);
                float2 heightmapUV = PaintContextUVToHeightmapUV(i.uv);

				// out of bounds multiplier
                float oob = all(saturate(brushUV) == brushUV) ? 1.0f : 0.0f;
                float oldHeight = UnpackHeightmap(tex2D(_MainTex, heightmapUV));
				float brushStrength = BRUSH_STRENGTH * oob * UnpackHeightmap(tex2D(_BrushTex, brushUV));

				const float2 coords [4] = { {-1,0}, { 1,0}, {0, -1}, { 0, 1} };

                float hc = UnpackHeightmap(tex2D(_MainTex, heightmapUV));
				float hl = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[0] * _MainTex_TexelSize.xy));
				float hr = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[1] * _MainTex_TexelSize.xy));
				float ht = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[2] * _MainTex_TexelSize.xy));
				float hb = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[3] * _MainTex_TexelSize.xy));

				float targetHeight = 0.25f * ( hl + hr + ht + hb );

				float minTarget = min(oldHeight, targetHeight);
				float maxTarget = max(oldHeight, targetHeight);

				targetHeight = lerp(minTarget, maxTarget, BRUSH_MODE);

				return PackHeightmap(lerp(oldHeight, targetHeight, brushStrength));
            }
            ENDCG
        }
    }
    Fallback Off
}
