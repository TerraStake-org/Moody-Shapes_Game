Shader "Custom/EmotionHair"
{
    Properties
    {
        _MainTex ("Base Color", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _Wetness ("Wetness", Range(0,1)) = 0
        _Shine ("Shine Intensity", Range(0,3)) = 1
        _WaterColor ("Water Color", Color) = (0.8,0.9,1,0.5)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        sampler2D _MainTex, _NormalMap;
        half _Roughness, _Wetness, _Shine;
        fixed4 _WaterColor;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            // Optional vertex animation could go here
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Base material properties
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
           
            // Apply wetness effects
            half wetFactor = smoothstep(0.3, 0.7, _Wetness);
            o.Albedo = lerp(baseColor.rgb, _WaterColor.rgb, wetFactor * _WaterColor.a);
            o.Metallic = lerp(0, 0.8, wetFactor);
            o.Smoothness = lerp(_Roughness, 0.95, wetFactor) * _Shine;
            o.Alpha = baseColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
