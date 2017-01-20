Shader "Custom/MyShader" {
	Properties {
		_NumRange ("Num Range", Range (0, 1)) = 0.6
		_NumFloat ("Num Float", Float) = 0.1
		_NumInt ("Num Int", Int) = 233
		_Color ("Color", Color) = (1,1,1,1)
		_Vector ("Vector", Vector) = (1,1,1,1)
		[NoScaleOffset]
		_Texture ("Texture", 2D) = "" {}
		_Cubemap ("Cubemap", Cube) = "black" {}
		_3D ("3D", 3D) = "white" {}
		}
	SubShader {
		Pass{
		Cull Front
		ZWrite On
		}

        Pass {
			Cull Back
            Lighting On
			ZWrite Off
			SetTexture [_Texture] {}
        }
    }
	FallBack "Diffuse"
}