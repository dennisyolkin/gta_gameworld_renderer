float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

float3 LightSource = float3(10, 10, 10);
float AmbientLight = 0.15;

struct VertexShaderInput
{
    float4 Position  : POSITION;
    float3 Normal    : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position        : POSITION;
    float LightingFactor   : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
   VertexShaderOutput output;
   float4 worldPosition = mul(input.Position, xWorld);
   float4x4 preViewProjection = mul(xView, xProjection);

   output.Position = mul(worldPosition, preViewProjection);

   float3 Normal = mul(input.Normal, xWorld);
   float3 lightVector = normalize(LightSource - worldPosition);
   output.LightingFactor = saturate(dot(input.Normal, lightVector));

   return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
   float4 color = float4(0, 1, 0, 1);
   color.rgb *= (input.LightingFactor + AmbientLight);
   return color;
}

technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
