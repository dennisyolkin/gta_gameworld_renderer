float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

float xOvercast;
float xTime;

Texture xTexture;

sampler TextureSampler = sampler_state 
{
   texture = <xTexture>;
   magfilter = LINEAR;
   minfilter = LINEAR;
   mipfilter=LINEAR;
   AddressU = mirror;
   AddressV = mirror;
};


// =============================== Sky technique ===================================
struct SkyVSOutput
{
   float4 Position         : POSITION;
   float2 TextureCoords    : TEXCOORD0;
   float4 ObjectPosition   : TEXCOORD1;
};


SkyVSOutput SkyVertexShader(float4 inPos : Position, float2 inTexCoords : TEXCOORD0)
{
   SkyVSOutput output;

   float4x4 preViewProjection = mul(xView, xProjection);
   float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

   output.Position = mul(inPos, preWorldViewProjection);
   output.ObjectPosition = inPos;
   output.TextureCoords = inTexCoords;

   return output;
}


float4 SkyPixelShader(SkyVSOutput input) : COLOR0
{
    float4 topColor = float4(0.3f, 0.3f, 0.8f, 1);
    float4 bottomColor = 1;
    
    float4 baseColor = lerp(bottomColor, topColor, saturate( (input.ObjectPosition.y) / 0.4f) );
    float cloudValue = tex2D(TextureSampler, input.TextureCoords).r;

    return lerp(baseColor, 1, cloudValue);
}
// =====================================================================================



// ================================ PerlinNoise technique ==============================
struct PNVertexToPixel
{    
    float4 Position         : POSITION;
    float2 TextureCoords    : TEXCOORD0;
};

struct PNPixelToFrame
{
    float4 Color : COLOR0;
};

PNVertexToPixel PerlinVS(float4 inPos : POSITION, float2 inTexCoords: TEXCOORD)
{    
    PNVertexToPixel Output = (PNVertexToPixel)0;
    
    Output.Position = inPos;
    Output.TextureCoords = inTexCoords;
    
    return Output;
}


PNPixelToFrame PerlinPS(PNVertexToPixel PSIn)
{
    PNPixelToFrame Output = (PNPixelToFrame)0;
    
    float2 move = float2(0, 1);
    float4 perlin = tex2D(TextureSampler, (PSIn.TextureCoords)      + xTime * move) / 2;
    perlin       += tex2D(TextureSampler, (PSIn.TextureCoords) * 2  + xTime * move) / 4;
    perlin       += tex2D(TextureSampler, (PSIn.TextureCoords) * 4  + xTime * move) / 8;
    perlin       += tex2D(TextureSampler, (PSIn.TextureCoords) * 8  + xTime * move) / 16;
    perlin       += tex2D(TextureSampler, (PSIn.TextureCoords) * 16 + xTime * move) / 32;
    perlin       += tex2D(TextureSampler, (PSIn.TextureCoords) * 32 + xTime * move) / 32;

    Output.Color = 1.0f - pow(perlin, xOvercast) * 2.0f;
    return Output;
}
// =====================================================================================


technique Sky
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 SkyVertexShader();
        PixelShader = compile ps_2_0  SkyPixelShader();
    }
}


technique PerlinNoise
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 PerlinVS();
        PixelShader = compile ps_2_0 PerlinPS();
    }
}
