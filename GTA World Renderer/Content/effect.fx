float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

Texture xTexture;   // используется при отрисовке техникой Textured
float4 xSolidColor; // используется при отрисовке техникой SolidColored

// Веса текстуры и цвета вершины при отрисовке с помощью Textured-техники
const float TextureColorWeight   = 0.9;
const float VertexColorWeight    = 0.25;

sampler TextureSampler = sampler_state { 
   texture = <xTexture>;
   magfilter = LINEAR;
   minfilter = LINEAR;
   mipfilter=LINEAR;
   AddressU = wrap;
   AddressV = wrap;
};


// ==================== Common data structures =======================
struct CommonVSInput
{
   float4 Position  : POSITION;
   float3 Normal    : NORMAL0;
};


struct CommonVSOutput
{
   float4 Position         : POSITION;
};



// ==================== Textured data structures =======================
struct TexturedVSInput
{
   CommonVSInput  Common;
   float2         TexCoords      : TEXCOORD0;
   float4         Color          : COLOR0;
};

struct TexturedVSOutput
{
   CommonVSOutput  Common;
   float2          TexCoords      : TEXCOORD0;
   float4          Color          : COLOR0;
};
// ====================================================================

// ==================== Colored data structures =======================
struct ColoredVSInput
{
   float4         Position   : POSITION;
   float4         Color      : COLOR0;
};

struct ColoredVSOutput
{
   float4          Position   : POSITION;
   float4          Color      : COLOR0;
};
// ====================================================================

inline CommonVSOutput CommonVertexShader(CommonVSInput input)
{
   CommonVSOutput output;

   float4x4 preViewProjection = mul(xView, xProjection);
   float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

   output.Position = mul(input.Position, preWorldViewProjection);

   return output;
}


// ============= Solid Colored shaders  =====================================

float4 SolidColoredPixelShader(CommonVSOutput input) : COLOR0
{
   float4 color = xSolidColor;
   return color;
}


// ============= Textured shaders  =====================================
TexturedVSOutput TexturedVertexShader(TexturedVSInput input)
{
   TexturedVSOutput output;
   output.Common = CommonVertexShader(input.Common);
   output.TexCoords = input.TexCoords;
   output.Color = input.Color;
   return output;
}

float4 TexturedPixelShader(TexturedVSOutput input) : COLOR0
{
   float4 color = tex2D(TextureSampler, input.TexCoords);
   color.rgb *= TextureColorWeight;
   color.rgb += input.Color * VertexColorWeight;
   return color;
}


// ============= Colored shaders  =====================================
ColoredVSOutput ColoredVertexShader(ColoredVSInput input)
{
   ColoredVSOutput output;
   float4 worldPosition = mul(input.Position, xWorld);
   float4x4 preViewProjection = mul(xView, xProjection);

   output.Position = mul(worldPosition, preViewProjection);
   output.Color = input.Color;
   return output;
}

float4 ColoredPixelShader(ColoredVSOutput input) : COLOR0
{
   float4 color = input.Color;
   return color;
}


// ====================================================================
technique SolidColored // Отрисовка сплошным заданным цветом
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 CommonVertexShader();
        PixelShader = compile ps_2_0  SolidColoredPixelShader();
    }
}

technique Colored // Отрисовка, используя цвета каждой вершины
{
   pass Pass1
   {
     VertexShader = compile vs_2_0 ColoredVertexShader();
     PixelShader = compile ps_2_0  ColoredPixelShader();
   }
}

technique Textured // Отрисовка с наложением текстуры
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 TexturedVertexShader();
        PixelShader = compile ps_2_0  TexturedPixelShader();
    }
}
