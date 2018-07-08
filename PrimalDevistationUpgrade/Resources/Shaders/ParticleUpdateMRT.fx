float4x4 viewProjection  : ViewProjection;
texture PositionTexture;
texture VelocityTexture;
texture TerrainTexture;
float2 RatioWH;
float2 Gravity;
float Friction;
float FrameDelta;
float4 Forces[4];
float NumForces=4;

sampler PositionSampler = sampler_state
{
    Texture = (PositionTexture);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

sampler VelocitySampler = sampler_state
{
    Texture = (VelocityTexture);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

sampler TerrainSampler = sampler_state
{
    Texture = (TerrainTexture);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

struct VS_Output
{
    float4 Position: POSITION;
    float2 TexCoord : TEXCOORD0;
};

struct PS_Output
{
	float4 Velocity: COLOR0;
    float4 Position: COLOR1;    
};

bool checkCollision(float x, float y)
{
	float4 col = tex2D(TerrainSampler, float2(x*RatioWH.x,y*RatioWH.y));
	if (col.w>0.5){ return true; }
	else { return false; }
}

PS_Output applyTerrainCollision(PS_Output input, float2 texCoord)
{
	/// First get the ineger (pixel locations) of the old and new positions
	float fpOldX = input.Position.x;
	float fpOldY = input.Position.y;
	float fpNewX = fpOldX + input.Velocity.x;
	float fpNewY = fpOldY + input.Velocity.y;
	int oldX = (int)fpOldX;
	int oldY = (int)fpOldY;
	int newX = (int)fpNewX;
	int newY = (int)fpNewY;
	//float random = texCoord.x;
	//int intRandom = random*10000;
	//if ((intRandom%2)==0){random=-random;}

	// If we have actually moved a pixel in the last frame (could have been less!)
	if (oldX != newX)
	{
		// If we collide at this new X position we need to rebound
		if (checkCollision(newX, oldY))
		{
			input.Velocity.x = (-input.Velocity.x * Friction);//+(random);
			fpNewX = fpOldX + input.Velocity.x;
		}                
	}

	// If we have actually moved a pixel in the last frame (could have been less!)
	if (oldY != newY)
	{
		// If we collide at this new X position we need to rebound
		if (checkCollision(oldX, newY))
		{
			input.Velocity.y = (-input.Velocity.y * Friction);//+(random);
			fpNewY = fpOldY + input.Velocity.y;
		}
	}

	// Finally set the new position
	input.Position.x = fpNewX;
	input.Position.y = fpNewY;
	return input;
}

VS_Output VertexShader(float4 Position: POSITION, float2 TexCoord : TEXCOORD0)
{
    VS_Output Out;
    Out.Position = mul(Position, viewProjection);
    Out.TexCoord = TexCoord;    
    return Out;
}

float vecLenSquare(float2 vec)
{
	return (vec.x*vec.x)+(vec.y*vec.y);
}

PS_Output PixelShader(float2 texCoord : TEXCOORD0)
{
    PS_Output Out;	    
   
	Out.Position = tex2D(PositionSampler, texCoord);	
	Out.Velocity = tex2D(VelocitySampler, texCoord);
	Out.Velocity.w+=FrameDelta;	
	bool forced = false;
	
	
	for(int i=0; i<NumForces; i++)
	{
		if (Forces[i].z!=0)
		{
			float2 diff = Forces[i].xy - Out.Position.xy;			
			float len = vecLenSquare(diff);	
			float absForce = abs(Forces[i].z);
			float reachSqr = Forces[i].w*Forces[i].w;
			if (len<10){len=10;}
			
			if (len<reachSqr)	
			{
				float2 normaliszed = normalize(diff);	
				
				//if (len<Forces[i].w*Forces[i].w){ Out.Position.xy = Forces[i].xy + (-normaliszed*(Forces[i].w));	}			
				
				Out.Velocity.xy += (normaliszed*(Forces[i].z+(texCoord.x/2.0)));		
				
				int max = absForce*100;					
				Out.Velocity.x = clamp(Out.Velocity.x,-max,max);
				Out.Velocity.y = clamp(Out.Velocity.y,-max,max);			
				
				forced = true;
				Out.Velocity.w=1;
			}
		}
	}
	
	if (!forced)
	{		
		Out.Velocity.x *= 0.99; Out.Velocity.y *= 0.99;
		Out.Velocity.x += Gravity.x; Out.Velocity.y += Gravity.y;			
	}
	
	return applyTerrainCollision(Out,texCoord);
}

technique ParticleUpdate
{
    pass Pass1
    {
		alphablendenable=false;
		ZWriteEnable = false;
		VertexShader = compile vs_3_0 VertexShader();
        PixelShader = compile ps_3_0 PixelShader();
    }
}



