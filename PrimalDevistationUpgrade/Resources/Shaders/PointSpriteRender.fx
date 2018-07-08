float4x4 View;
float4x4 Projection;
float4x4 World;
float ParticleSize;
float2 CameraPos;
float2 ZoomCentre;
float Zoom;

texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

struct VS_OUTPUT
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float1 Size : PSIZE;    
};

VS_OUTPUT VertexShader(	

	float4 Position : POSITION, 
    float4 Color : COLOR0,
    float1 Size : PSIZE
        
    )
{
    VS_OUTPUT Output;

	Position.xy -= CameraPos;	
	Position.xy = ZoomCentre+(Position*Zoom);	
    Output.Position = mul(mul(Position, View), Projection); 
    Output.Diffuse = Color;
    Output.Size = Size; 

    return Output;
}

float4 PixelShader(float2 texCoord : TEXCOORD0, float4 Diffuse : COLOR0) : COLOR0
{
    return tex2D(Sampler, texCoord)* float4(Diffuse.xyz, 0.8);
}

technique PointSpriteRender
{
    pass
    {    
        PointSpriteEnable = true;       

        //SrcBlend = SrcAlpha;
        //DestBlend = SrcAlpha;
        ZWriteEnable = false;
        
        VertexShader = compile vs_2_0 VertexShader();
        PixelShader = compile ps_2_0 PixelShader();
    }
}