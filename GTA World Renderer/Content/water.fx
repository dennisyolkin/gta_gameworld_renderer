float4x4 xView;
float4x4 xProjection;

float4 waterColor = float4(32 / 255.0, 57 / 255.0, 87 / 255.0, 1);

struct VSOutput
{
   float4 Position         : POSITION;
};


VSOutput WaterVertexShader(float4 inPos : POSITION)
{
   VSOutput result;

   float4x4 preViewProjection = mul(xView, xProjection);
   result.Position =  mul(inPos, preViewProjection);

   return result;
}


float4 WaterPixelShader(VSOutput input) : COLOR0
{
   return waterColor;
}


technique Water
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 WaterVertexShader();
        PixelShader = compile ps_2_0  WaterPixelShader();
    }
}

