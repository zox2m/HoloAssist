// Copyright (C) 2024 VIRNECT CO., LTD.
// All rights reserved.

Shader "VIRNECT/BackgroundYUVShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _UvTopLeftRight("UV of top corners", Vector) = (0, 1, 1, 1)
        _UvBottomLeftRight("UV of bottom corners", Vector) = (0, 0, 1, 0)
    }
    // For GLES3 or GLES2 on device
    SubShader
    {
        Pass
        {
            ZWrite Off
            Cull Off

            GLSLPROGRAM

            #pragma only_renderers gles3 gles

            // #ifdef SHADER_API_GLES3 cannot take effect because
            // #extension is processed before any Unity defined symbols.
            // Use "enable" instead of "require" here, so it only gives a
            // warning but not compile error when the implementation does not
            // support the extension.
            #extension GL_OES_EGL_image_external_essl3 : enable
            #extension GL_OES_EGL_image_external : enable

            uniform vec4 _UvTopLeftRight;
            uniform vec4 _UvBottomLeftRight;

            // Use the same method in UnityCG.cginc to convert from gamma to
            // linear space in glsl.
            vec3 GammaToLinearSpace(vec3 color)
            {
                return color *
                    (color * (color * 0.305306011 + 0.682171111) + 0.012522878);
            }

            #ifdef VERTEX

            varying vec2 textureCoord;
            varying vec2 uvCoord;

            void main()
            {
                vec2 uvTop = mix(_UvTopLeftRight.xy,
                                 _UvTopLeftRight.zw,
                                 gl_MultiTexCoord0.x);
                vec2 uvBottom = mix(_UvBottomLeftRight.xy,
                                    _UvBottomLeftRight.zw,
                                    gl_MultiTexCoord0.x);
                textureCoord = mix(uvTop, uvBottom, gl_MultiTexCoord0.y);
                uvCoord = vec2(gl_MultiTexCoord0.x, gl_MultiTexCoord0.y);

                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }

            #endif

            #ifdef FRAGMENT
            varying vec2 textureCoord;
            varying vec2 uvCoord;
            uniform samplerExternalOES _MainTex;

            void main()
            {
                vec3 mainTexColor;

                #ifdef SHADER_API_GLES3
                mainTexColor = texture(_MainTex, textureCoord).rgb;
                #else
                mainTexColor = textureExternal(_MainTex, textureCoord).rgb;
                #endif

#ifndef UNITY_COLORSPACE_GAMMA

                mainTexColor = GammaToLinearSpace(mainTexColor);
#endif
                gl_FragColor = vec4(mainTexColor, 1.0);
            }

            #endif

            ENDGLSL
        }
    }

    FallBack Off
}