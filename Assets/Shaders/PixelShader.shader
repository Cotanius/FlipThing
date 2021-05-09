Shader "Custom/PixelShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _ScanTex("ScanTex (Angle Texture)", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert fullforwardshadows 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _ScanTex;

        struct Input
        {
            float2 uv_MainTex;
        };
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // https://forum.unity.com/threads/rotating-mesh-in-vertex-shader.501709/
        float4 RotateAroundXInDegrees(float4 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float4(mul(m, vertex.yz), vertex.xw).xzyw;
        }

        void vert(inout appdata_full v) {
            float angle = tex2Dlod(_ScanTex, float4(v.texcoord1.xy, 0, 0));            
            float3 col = v.color.rgb;
            float3 localPixelPos = -(col - .5) * 2;

            v.vertex += RotateAroundXInDegrees(float4(localPixelPos, 0), angle * 180);
        }



        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex)* _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = c.rgb ;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
