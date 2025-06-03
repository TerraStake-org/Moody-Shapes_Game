Shader "Custom/AccessoryHair"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _FlowMap ("Flow Map", 2D) = "gray" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _FlowIntensity ("Flow Intensity", Range(0,2)) = 0.5
        _Color ("Hair Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.1
        _Smoothness ("Smoothness", Range(0,1)) = 0.8
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionIntensity ("Emission Intensity", Range(0,5)) = 0
        
        [Header(Emotion Response)]
        _EmotionColorTint ("Emotion Color Tint", Color) = (1,1,1,1)
        _EmotionFlowBoost ("Emotion Flow Boost", Range(0,3)) = 1
        _EmotionGlow ("Emotion Glow", Range(0,2)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _FlowMap;
        sampler2D _NormalMap;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_FlowMap;
            float3 worldPos;
            float3 worldNormal;
        };

        half _FlowIntensity;
        fixed4 _Color;
        half _Metallic;
        half _Smoothness;
        fixed4 _EmissionColor;
        half _EmissionIntensity;
        
        // Emotion properties
        fixed4 _EmotionColorTint;
        half _EmotionFlowBoost;
        half _EmotionGlow;

        void vert(inout appdata_full v)
        {
            // Add subtle vertex animation for hair movement
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float wave = sin(_Time.y * 2 + worldPos.x * 0.5) * 0.02;
            v.vertex.xyz += v.normal * wave * _FlowIntensity;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Sample flow map for hair direction
            float2 flow = tex2D(_FlowMap, IN.uv_FlowMap).rg * 2 - 1;
            float flowMagnitude = length(flow);
            
            // Animate UV based on flow and emotion
            float emotionTimeScale = 1 + _EmotionFlowBoost;
            float2 scrolledUV = IN.uv_MainTex + flow * _FlowIntensity * _Time.y * emotionTimeScale;
            
            // Sample main texture with scrolled UVs
            fixed4 c = tex2D(_MainTex, scrolledUV) * _Color;
            
            // Apply emotion color tinting
            c.rgb *= _EmotionColorTint.rgb;
            
            // Sample normal map
            float3 normal = UnpackNormal(tex2D(_NormalMap, scrolledUV));
            
            // Output
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Normal = normal;
            
            // Emotion-based emission
            float emotionEmission = _EmotionGlow * _EmissionIntensity;
            o.Emission = _EmissionColor.rgb * emotionEmission * (1 + flowMagnitude);
            
            o.Alpha = c.a;
        }
        ENDCG
    }
    
    // Fallback for older hardware
    FallBack "Standard"
}
