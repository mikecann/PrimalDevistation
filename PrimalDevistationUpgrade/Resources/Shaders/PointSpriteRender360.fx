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
texture ColorsTexture;
texture AgeTexture;

texture ParticleTex0;
texture ParticleTex1;
texture ParticleTex2;
texture ParticleTex3;
texture ParticleTex4;
texture ParticleTex5;
texture ParticleTex6;
texture ParticleTex7;
texture ParticleTex8;
texture ParticleTex9;

sampler particleSamplers[10] = 
{ 
	sampler_state { texture = <ParticleTex0>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; }, 
	sampler_state { texture = <ParticleTex1>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex2>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex3>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex4>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex5>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex6>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex7>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex8>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; },
	sampler_state { texture = <ParticleTex9>; MinFilter = Linear; MagFilter = Linear; MipFilter = Linear; AddressU = Clamp; AddressV = Clamp; } 
};



    
    
    
    
    


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

sampler2D ColorsTexSampler = sampler_state
{
    Texture = (ColorsTexture);
};

sampler2D AgeTexSampler = sampler_state
{
    Texture = (AgeTexture);
};

struct VS_OUTPUT
{
    float4 Position   : POSITION;
    float1 Size : PSIZE;  
    float2 TextureCoordinate :   TEXCOORD0;
    float4 Rotation : COLOR0;
    float TexIndex :   DEPTH0;
    float4 Color : COLOR1;
};

// Pixel shader input structure for particles that can rotate.
struct RotatingPixelShaderInput
{

    float4 Rotation : COLOR0;
    
    float4 Color : COLOR1;
    float TexIndex :   DEPTH0;
    
#ifdef XBOX
    float2 TextureCoordinate : SPRITETEXCOORD;
#else
    float2 TextureCoordinate : TEXCOORD0;
#endif

};

VS_OUTPUT VertexShader(float2 PositionCoord : TEXCOORD0,  float Size : PSIZE, float texIndex : DEPTH0  )
{
    VS_OUTPUT Output;
		
	float4 coord = float4(PositionCoord,0,0);
	float2 pos = tex2Dlod(PosTexSampler,coord );	
	float2 vel = tex2Dlod(VelTexSampler,coord );
	
	Output.TextureCoordinate = PositionCoord;
	Output.Position.x = pos.x;
	Output.Position.y = pos.y;	
	Output.Position.z = 0;
	Output.Position.w = 1;	
	Output.Position.xy -= CameraPos;	
	Output.Position.xy = ZoomCentre+(Output.Position.xy*Zoom);	
    Output.Position = mul(mul(Output.Position, View), Projection); 
    Output.TexIndex = texIndex;    
    
    // Compute a 2x2 rotation matrix.
    float c = cos(-vel.x*5);
    float s = sin(-vel.x*5);    
    float4 rotationMatrix = float4(c, -s, s, c);
    rotationMatrix *= 0.5;
    rotationMatrix += 0.5;
    Output.Rotation = rotationMatrix;
    
    float4 color=(float4)1;
    int lenSqr = (vel.x*vel.x)+(vel.y*vel.y);
	
	// As long as this particle isnt stationary
	if (lenSqr>2)
	{
		float age = tex2Dlod(AgeTexSampler, coord).x;
		
		if (age>3){age=3;}
		float s = lenSqr/(5.0+(age*60));
		
		if (s>1){s=1;}
		float toAdd = lerp(0,1,s);
		color.x += toAdd;
		color.y += toAdd;
		color.z += toAdd;
		color.w = age*3;
	}
    
    Output.Color = color*tex2Dlod(ColorsTexSampler, coord);
    
    float sze = Size*Zoom;
    if (sze<1){sze=1;}
    Output.Size = sze; 

    return Output;
}




//float4 PixelShader(float2 texCoord : TEXCOORD0,  float2 Velocity :   TEXCOORD1, float TexIndex :   DEPTH0) : COLOR0
float4 PixelShader(RotatingPixelShaderInput input) : COLOR0
{	
   float2 textureCoordinate = input.TextureCoordinate;

    // We want to rotate around the middle of the particle, not the origin,
    // so we offset the texture coordinate accordingly.
    textureCoordinate -= 0.5;
    
    // Apply the rotation matrix, after rescaling it back from the packed
    // color interpolator format into a full -1 to 1 range.
    float4 rotation = input.Rotation * 2 - 1;
    
    textureCoordinate = mul(textureCoordinate, float2x2(rotation));
    
    // Point sprites are squares. So are textures. When we rotate one square
    // inside another square, the corners of the texture will go past the
    // edge of the point sprite and get clipped. To avoid this, we scale
    // our texture coordinates to make sure the entire square can be rotated
    // inside the point sprite without any clipping.
    textureCoordinate *= sqrt(2);
    
    // Undo the offset used to control the rotation origin.
    textureCoordinate += 0.5;

	float4 color= (float4)0;
	int tex = (int)input.TexIndex;
	if (tex==0){color = tex2D(particleSamplers[1], textureCoordinate);}
	else if (tex==1){color = tex2D(particleSamplers[1], textureCoordinate);}
	else if (tex==2){color = tex2D(particleSamplers[2], textureCoordinate);}
	else if (tex==3){color = tex2D(particleSamplers[3], textureCoordinate);}
	else if (tex==4){color = tex2D(particleSamplers[4], textureCoordinate);}
	else if (tex==5){color = tex2D(particleSamplers[5], textureCoordinate);}
	else if (tex==6){color = tex2D(particleSamplers[6], textureCoordinate);}
	else if (tex==7){color = tex2D(particleSamplers[7], textureCoordinate);}
	else if (tex==8){color = tex2D(particleSamplers[8], textureCoordinate);}
	else if (tex==9){color = tex2D(particleSamplers[9], textureCoordinate);}
	
	return color*input.Color;

    //return color * tex2D(ColorsTexSampler, TextureCoordinate);
}

technique RenderVT
{
    pass
    {                
        PointSpriteEnable = true;      
        ZWriteEnable = false;
        
        VertexShader = compile vs_3_0 VertexShader();
        PixelShader = compile ps_3_0 PixelShader();
    }
}