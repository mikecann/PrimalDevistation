float4x4 View;
float4x4 Projection;
float4x4 World;
float ParticleSize;
float2 CameraPos;
float2 ZoomCentre;
float Zoom;

texture ParticleTexture;
texture PositionsTexture;
texture VelocitiesTexture;

sampler ParticleTextureSampler = sampler_state
{
    Texture = (ParticleTexture);
};

sampler VelTexSampler = sampler_state
{
    Texture = (VelocitiesTexture);
};

sampler2D PosTexSampler = sampler_state
{
    Texture = (PositionsTexture);
};

struct VS_OUTPUT
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float1 Size : PSIZE;  
    float2 TexCoord :   TEXCOORD0;
};

VS_OUTPUT VertexShader(float2 PositionCoord : TEXCOORD0,  float1 Size : PSIZE  )
{
    VS_OUTPUT Output;
		
	float4 coord = float4(PositionCoord,0,0);
	Output.Position = tex2Dlod(PosTexSampler,coord );		
	Output.Diffuse = tex2Dlod(VelTexSampler, coord); 
	Output.TexCoord = PositionCoord;
	
	int lenSqr = (Output.Diffuse.x*Output.Diffuse.x)+(Output.Diffuse.y*Output.Diffuse.y);
	Output.Diffuse.x = Output.Position.z;
	Output.Diffuse.y = Output.Position.w;	
	
	// As long as this particle isnt stationary
	if (lenSqr>2)
	{
		float time = Output.Diffuse.w;
		if (time>3){time=3;}
		float s = lenSqr/(5.0+(time*60));
		
		if (s>1){s=1;}
		float toAdd = lerp(0,1,s);
		Output.Diffuse.x += toAdd;
		Output.Diffuse.y += toAdd;
		Output.Diffuse.z += toAdd;
	}
						
	Output.Position.z = 0;
	Output.Position.w = 1;	
	Output.Position.xy -= CameraPos;	
	Output.Position.xy = ZoomCentre+(Output.Position.xy*Zoom);	
    Output.Position = mul(mul(Output.Position, View), Projection); 
    
    float sze = Size*Zoom;
    if (sze<1){sze=1;}
    Output.Size = sze; 

    return Output;
}


float4 PixelShader(float2 texCoord : TEXCOORD0, float4 Diffuse : COLOR0) : COLOR0
{
    return tex2D(ParticleTextureSampler, texCoord)* float4(Diffuse.xyz, 0.9);
}

technique RenderVT
{
    pass
    {    
        PointSpriteEnable = true;        
       
        //SrcBlend = SrcAlpha;
        //DestBlend = SrcAlpha;
        ZWriteEnable = false;
        
        VertexShader = compile vs_3_0 VertexShader();
        PixelShader = compile ps_3_0 PixelShader();
    }
}