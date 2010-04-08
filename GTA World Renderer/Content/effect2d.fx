/*
Эффект-файл для отрисовки "двухмерной карты".
Все объекты проецируются на плоскость OXZ и рисуется WireFrame.
*/


float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float4   xColor;        // цвет, которым отрисовывать контуры объекта



float4 VertexShaderProgram(float4 inPos : POSITION) : POSITION
{
   float4x4 viewProjection = mul( xView, xProjection );
   float4 pos = mul( inPos, xWorld );
   pos.y = 0;
   pos = mul( pos, viewProjection );
   return pos;
}


float4 PixelShaderProgram() : COLOR0
{
   return xColor;
}


technique Basic
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderProgram();
        PixelShader = compile ps_2_0  PixelShaderProgram();
    }
}