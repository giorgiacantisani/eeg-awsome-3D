Shader "Custom/ElectrodeDisplay"
{
    Properties
    {
        _MainTex ("Electrode Display RT (RGB)", 2D) = "white" {}
        _Electrode ("Electrode#", Range(0,31)) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        // Cull Off //ZWrite Off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        // #pragma surface surf Standard fullforwardshadows alpha:blend
        // #pragma surface surf Unlit
        #pragma surface surf Unlit alpha:blend// alphatest:_Cutoff

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

        float _Electrode;
        int _MainTexWidth = 32;
        int _MainTexHeight = 32;
        int _WriteX;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        struct SurfaceOutputUnlit
        {
            half3 Normal;
            half3 Emission;
            half3 Albedo;
            half Alpha;
        };

        inline void LightingUnlit_GI (SurfaceOutputUnlit s, UnityGIInput data, inout UnityGI gi) {}

        inline half4 LightingUnlit (SurfaceOutputUnlit s, half3 viewDir, UnityGI gi)
        {
            // TODO: also could use Emission as output with Albedo=0 and Standard shader
            return half4(s.Albedo, s.Alpha);
        }

        void surf (Input IN, inout SurfaceOutputUnlit o)
        {
            float2 uv = float2(_WriteX/32.0 - IN.uv_MainTex.y, _Electrode/32.0);

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, uv);
            o.Albedo = c.rgb * 0.125;
            // o.Alpha = pow((1-IN.uv_MainTex.y)*0.2, 0.5) * pow(IN.uv_MainTex.y*0.2, 0.5);
            o.Alpha = 1;//pow((1-IN.uv_MainTex.y)*0.9 * IN.uv_MainTex.y*0.9, 0.95);
            //IN.uv_MainTex.y < 0.5 ? saturate(IN.uv_MainTex.y * 0.1) : 1-saturate((1-IN.uv_MainTex.y) * 0.1);//-IN.worldPos.y + sin(length(IN.worldPos.xz));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
