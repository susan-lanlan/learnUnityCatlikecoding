#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct DirectionalShadowData {
	float strength;
	int tileIndex;
};

float SampleDirectionalShadowAtlas (float3 positionSTS) {
	return SAMPLE_TEXTURE2D_SHADOW(
        _DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS
	);
}
float GetDirectionalShadowAttenuation (DirectionalShadowData data, Surface surfaceWS) {
    if (data.strength <= 0.0) {
		return 1.0;
	}
    //通过阴影转换矩阵和表面位置得到在阴影纹理（图块）空间的位置，然后对图集进行采样
	float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex], 
        float4(surfaceWS.position, 1.0)).xyz;
	float shadow = SampleDirectionalShadowAtlas(positionSTS);
    //最终阴影衰减值是阴影强度和衰减因子的插值
    //shadow决定了到达表面的光照量，片元完全在阴影中则为0，片元完全不在阴影中则为0。
    //strength影响的是透明材质
	return lerp(1.0, shadow, data.strength);
}
#endif