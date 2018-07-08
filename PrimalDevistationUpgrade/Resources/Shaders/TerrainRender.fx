// Pixel shader extracts the brighter areas of an image.
// This is the first step in applying a bloom postprocess.

sampler TextureSampler : register(s0) = sampler_state
{
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};


texture ExplosionTexture;

sampler ExplosionSampler = sampler_state
{
    Texture = (ExplosionTexture);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

texture IndestructableTexture;

sampler IndestructableSampler = sampler_state
{
    Texture = (IndestructableTexture);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};


float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{

	float4 outCol = float4(1,0,0,0);
	float4 explosinCol = tex2D(ExplosionSampler, texCoord);
	if (explosinCol.x!=0)
	{
		outCol = tex2D(TextureSampler, texCoord);
	}


	float4 indestr = tex2D(IndestructableSampler, texCoord);
	if (indestr.w!=0){ outCol = indestr;  }
	
	return outCol;
}


technique TerrainRender
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
