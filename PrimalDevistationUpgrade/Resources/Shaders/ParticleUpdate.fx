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
};

sampler VelocitySampler = sampler_state
{
    Texture = (VelocityTexture);
};

sampler TerrainSampler = sampler_state
{
    Texture = (TerrainTexture);
};

struct VS_Output
{
    float4 Position: POSITION;
    float2 TexCoord : TEXCOORD0;
};

VS_Output VertexShader(float4 Position: POSITION, float2 TexCoord : TEXCOORD0)
{
    VS_Output Out;
    Out.Position = mul(Position, viewProjection);
    Out.TexCoord = TexCoord;    
    return Out;
}

bool checkCollision(float x, float y)
{
	float4 col = tex2D(TerrainSampler, float2(x*RatioWH.x,y*RatioWH.y));
	if (col.w>0.5){ return true; }
	else { return false; }
}

float vecLenSquare(float2 vec)
{
	return (vec.x*vec.x)+(vec.y*vec.y);
}

float4 applyTerrainCollisionVel(float4 Position, float4 Velocity, float2 texCoord)
{
	/// First get the ineger (pixel locations) of the old and new positions
	float fpOldX = Position.x;
	float fpOldY = Position.y;
	float fpNewX = fpOldX + Velocity.x;
	float fpNewY = fpOldY + Velocity.y;
	int oldX = (int)fpOldX;
	int oldY = (int)fpOldY;
	int newX = (int)fpNewX;
	int newY = (int)fpNewY;

	// If we have actually moved a pixel in the last frame (could have been less!)
	if (oldX != newX)
	{
		// If we collide at this new X position we need to rebound
		if (checkCollision(newX, oldY))
		{
			Velocity.x = (-Velocity.x * Friction);
		}                
	}

	// If we have actually moved a pixel in the last frame (could have been less!)
	if (oldY != newY)
	{
		// If we collide at this new X position we need to rebound
		if (checkCollision(oldX, newY))
		{
			Velocity.y = (-Velocity.y * Friction);
		}
	}

	return Velocity;
}


float4 applyTerrainCollisionPos(float4 Position, float4 Velocity, float2 texCoord)
{
	/// First get the ineger (pixel locations) of the old and new positions
	float fpOldX = Position.x;
	float fpOldY = Position.y;
	float fpNewX = fpOldX + Velocity.x;
	float fpNewY = fpOldY + Velocity.y;
	int oldX = (int)fpOldX;
	int oldY = (int)fpOldY;
	int newX = (int)fpNewX;
	int newY = (int)fpNewY;

	// If we have actually moved a pixel in the last frame (could have been less!)
	if (oldX != newX)
	{
		// If we collide at this new X position we need to rebound
		if (checkCollision(newX, oldY))
		{
			fpNewX = fpOldX - Velocity.x;
		}                
	}

	// If we have actually moved a pixel in the last frame (could have been less!)
	if (oldY != newY)
	{
		// If we collide at this new X position we need to rebound
		if (checkCollision(oldX, newY))
		{
			fpNewY = fpOldY - Velocity.y;
		}
	}

	// Finally set the new position
	Position.x = fpNewX;
	Position.y = fpNewY;
	return Position;
}


float4 PSUpdateVelocity(float2 texCoord : TEXCOORD0) : COLOR
{   
	float4 Position = tex2D(PositionSampler, texCoord);	
	float4 Velocity = tex2D(VelocitySampler, texCoord);
	Velocity.w+=FrameDelta;	
	bool forced = false;	
	
	for(int i=0; i<NumForces; i++)
	{
		if (Forces[i].z!=0)
		{
			float2 diff = Forces[i].xy - Position.xy;			
			float len = vecLenSquare(diff);	
			float absForce = abs(Forces[i].z);
			float reachSqr = Forces[i].w*Forces[i].w;
			if (len<10){len=10;}
			
			if (len<reachSqr)	
			{
				float2 normaliszed = normalize(diff);	
				
				//if (len<Forces[i].w*Forces[i].w){ Position.xy = Forces[i].xy + (-normaliszed*(Forces[i].w));	}			
				
				Velocity.xy += (normaliszed*(Forces[i].z+(texCoord.x/2.0)));		
				
				int max = absForce*100;					
				Velocity.x = clamp(Velocity.x,-max,max);
				Velocity.y = clamp(Velocity.y,-max,max);			
				
				forced = true;
				Velocity.w=1;
			}
		}
	}
	
	if (!forced)
	{		
		Velocity.x *= 0.99; Velocity.y *= 0.99;
		Velocity.x += Gravity.x; Velocity.y += Gravity.y;			
	}
	
	return applyTerrainCollisionVel(Position,Velocity,texCoord);
}


float4 PSUpdatePosition(float2 texCoord : TEXCOORD0) : COLOR0
{  
	float4 Position = tex2D(PositionSampler, texCoord);	
	float4 Velocity = tex2D(VelocitySampler, texCoord);
	return applyTerrainCollisionPos(Position,Velocity,texCoord);
}

technique UpdatePos
{
    pass Pass1
    {
		alphablendenable=false;
		ZWriteEnable = false;
		VertexShader = compile vs_3_0 VertexShader();
        PixelShader = compile ps_3_0 PSUpdatePosition();
    }
}


technique UpdateVel
{
    pass Pass1
    {
		alphablendenable=false;
		ZWriteEnable = false;
		VertexShader = compile vs_3_0 VertexShader();
        PixelShader = compile ps_3_0 PSUpdateVelocity();
    }
}



