
sampler texSampler;
Texture warpMap;
sampler warpSampler = sampler_state{texture = <warpMap>;};

float mag = 1;
float radius = 0.5;
float2 center = float2(.5,.5);
float growth  = 0;

struct PS_INPUT
{
    float2 TexCoord: TEXCOORD0;
};

float4 WarpBufferPS(PS_INPUT Input) : COLOR {
   

  float2 tex = Input.TexCoord;
  float2 displace = center - tex;
  float scale = length(displace);
  radius = abs(sin(growth*8)*growth);
  float range = clamp(1-(scale/radius),0,1);
  displace *= range * mag;
  float4 col = tex2D(warpSampler,tex);
  col.r += displace.x;
  col.g += displace.y;
  return col;
}

float4 WarpRenderPS(PS_INPUT Input) : COLOR {   	
  float2 tex = Input.TexCoord;
  float4 displaceSample = tex2D(warpSampler,tex);
  float2 displacement = float2(displaceSample.r, displaceSample.g);  
  return tex2D(texSampler,tex + displacement);
}

technique WarpBuffer 
{
	pass p0 
	{
		PixelShader = compile ps_2_0 WarpBufferPS();
	}
}

technique WarpRender
{
	pass p0
	{
		PixelShader = compile ps_2_0 WarpRenderPS();
	}
}
