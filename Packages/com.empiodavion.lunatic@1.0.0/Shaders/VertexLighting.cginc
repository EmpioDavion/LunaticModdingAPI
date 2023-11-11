#ifndef LUNATIC_VERTEX_LIGHTING
#define LUNATIC_VERTEX_LIGHTING

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityLightingCommon.cginc"

void VertexLighting(fixed3 positionVS, fixed3 normalVS, fixed3 viewDirVS, int index, inout fixed3 diffuse, inout fixed3 spec)
{
    fixed3 lightDirVS;
    fixed attenuation;
    fixed distanceSq = 1.0;

    if (unity_LightPosition[index].w == 0.0)
    {
        attenuation = 1.0; // no attenuation
        lightDirVS = normalize(unity_LightPosition[index].xyz);
    }
    else // point or spot light
    {
        fixed3 toLight = unity_LightPosition[index].xyz - positionVS * unity_LightPosition[index].w;
        distanceSq = max(dot(toLight, toLight), 0.000001);
        lightDirVS = toLight * rsqrt(distanceSq);
        attenuation = 1.0 / (1.0 + distanceSq * unity_LightAtten[index].z); // linear attenuation 
    }

    fixed ndl = max(0.0, dot(normalVS, lightDirVS));
    fixed3 diffuseReflection = _Color.rgb * unity_LightColor[index].rgb * ndl * attenuation;

    // blinn phong
    fixed3 halfDirVS = normalize(lightDirVS + viewDirVS);
    fixed ndh = max(0.0, dot(normalVS, halfDirVS));
    fixed specular = pow(ndl, _Shininess * 128.0);
    fixed3 specularReflection = unity_LightColor[index].rgb * _SpecColor.rgb * specular / distanceSq;

    diffuse += diffuseReflection;
    spec += specularReflection;
}

void GetVertexLighting(fixed3 positionOS, fixed3 normalOS, out fixed3 diffuse, out fixed3 spec)
{
    fixed3 positionVS = UnityObjectToViewPos(positionOS);
    fixed3 normalVS = normalize(mul((fixed3x3)UNITY_MATRIX_IT_MV, normalOS));
    fixed3 viewDirVS = -normalize(positionVS);

    diffuse = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;
    spec = _Emission;

    for (int i = 0; i < 8; i++)
        VertexLighting(positionVS, normalVS, viewDirVS, i, diffuse, spec);
}

#endif
