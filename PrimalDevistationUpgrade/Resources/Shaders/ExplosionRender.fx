// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

sampler TextureSampler : register(s0);
texture IndestructableTexture;
float3 ExplosionData;
float2 RatioWH;


sampler IndestructableSampler = sampler_state
{
    Texture = (IndestructableTexture);
};

float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{  	    

	float pixelX = ExplosionData.x + (texCoord.x*ExplosionData.z);
	float pixelY = ExplosionData.y + (texCoord.y*ExplosionData.z);
	float2 coord = float2(pixelX*RatioWH.x,pixelY*RatioWH.y);

	float4 outCol;
	float4 indes = tex2D(IndestructableSampler, coord);

	if (indes.w!=0){ outCol = float4(1,1,1,0); }
	else { outCol = tex2D(TextureSampler, texCoord); }
    return outCol;
}

technique ExplosionRender
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
