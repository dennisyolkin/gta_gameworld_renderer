float4x4 xView;
float4x4 xProjection;
Texture xBumpTexture;
float4 xTime;

sampler BumpTextureSampler = sampler_state 
{
   texture = <xBumpTexture>;
   magfilter = LINEAR;
   minfilter = LINEAR;
   mipfilter=LINEAR;
   AddressU = mirror;
   AddressV = mirror;
};


struct VSOutput
{
   float4 Position         : POSITION;
   float2 BumpSamplingPos  : TEXCOORD0;
};


VSOutput WaterVertexShader(float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0)
{
   VSOutput result;

   float4x4 preViewProjection = mul(xView, xProjection);
   result.Position =  mul(inPos, preViewProjection);
   result.BumpSamplingPos = inTexCoords;

   return result;
}


float4 WaterPixelShader(VSOutput input) : COLOR0
{
   float4 color = tex2D(BumpTextureSampler, input.BumpSamplingPos - xTime);
   return color;
}


technique Water
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 WaterVertexShader();
        PixelShader = compile ps_2_0  WaterPixelShader();
    }
}

