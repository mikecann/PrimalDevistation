// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

sampler TextureSampler : register(s0);
float4x4 View;
float4x4 Projection;
float4x4 World;

struct VS_OUTPUT
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float4 TexCoords  : TEXCOORD0;
};

VS_OUTPUT VertexShader(	float4 Position : POSITION, float4 TexCoords : TEXCOORD0, float4 Color : COLOR0 )
{
    VS_OUTPUT Output;
    
    Output.Position = mul(mul(Position, View), Projection); 
    Output.Diffuse = Color;
    Output.TexCoords = TexCoords; 

    return Output;
}

float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{  	 	
	//return float4(texCoord,0,0);
    return tex2D(TextureSampler, texCoord);
}

technique ExplosionRender
{
    pass Pass1    
    {
		//VertexShader = compile vs_2_0 VertexShader();
        PixelShader = compile ps_2_0 PixelShader();
    }
}
