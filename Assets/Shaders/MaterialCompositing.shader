Shader "Hidden/MaterialCompositing"
{
    Properties
    {
        _AlbedoTex ("Albedo Texture", 2D) = "white" {}
        _NormalTex ("Normal Map", 2D) = "bump" {}
        _RoughnessTex ("Roughness Map", 2D) = "white" {}
        _Glossiness ("Glossiness", Range(0,1)) = 0.5
        _Tiling ("Tiling", Float) = 1.0
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _AlbedoTex;
            sampler2D _NormalTex;
            sampler2D _RoughnessTex;
            float _Glossiness;
            float _Tiling;
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Применяем масштабирование текстуры
                float2 tiledUV = i.uv * _Tiling;
                
                // Получаем цвет из альбедо
                fixed4 albedo = tex2D(_AlbedoTex, tiledUV);
                
                // Получаем нормаль из карты нормалей
                fixed4 normal = tex2D(_NormalTex, tiledUV);
                
                // Получаем шероховатость
                fixed roughness = tex2D(_RoughnessTex, tiledUV).r;
                
                // Корректируем шероховатость в соответствии с глянцевостью
                roughness = lerp(roughness, 1.0 - roughness, _Glossiness);
                
                // Кодируем информацию для использования в основном шейдере:
                // RGB - цвет альбедо
                // A - шероховатость/глянцевость
                
                // Создаем результирующий цвет
                fixed4 result = fixed4(albedo.rgb, roughness);
                
                // Сохраняем нормали в отдельную текстуру или в другой буфер, если нужно
                // (в данном случае мы просто используем основной цвет и шероховатость)
                
                return result;
            }
            ENDCG
        }
    }
} 