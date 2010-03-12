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

    float4 worldPosition = mul(input.Position, xWorld);
    float4 viewPosition = mul(worldPosition, xView);
    output.Position = mul(viewPosition, xProjection);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return float4(0, 1, 0, 1);
}

technique Simple
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
