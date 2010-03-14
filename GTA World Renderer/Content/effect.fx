float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;


struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
   VertexShaderOutput output;
   float4x4 preViewProjection = mul(xView, xProjection);
   float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

   output.Position = mul(input.Position, preWorldViewProjection);

   return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return float4(0, 0, 1, 1);
}

technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
