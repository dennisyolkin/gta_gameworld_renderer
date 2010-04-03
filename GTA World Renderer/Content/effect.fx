float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

Texture xTexture;   // используется при отрисовке техникой Textured
float4 xSolidColor; // используется при отрисовке техникой SolidColored

sampler TextureSampler = sampler_state { 
   texture = <xTexture>;
   magfilter = LINEAR;
   minfilter = LINEAR;
   mipfilter=LINEAR;
   AddressU = wrap;
   AddressV = wrap;
};


float3 LightSource = float3(10, 10, 10);
float AmbientLight = 0.30;


/*
TODO :: 
не повлиляет ли на эффективность использование CommonVertexShader и соответствующих Common-структур?
Возможно, в данном случае "дешевле" продублировать код.
*/

// ==================== Common data structures =======================
struct CommonVSInput
{
   float4 Position  : POSITION;
   float3 Normal    : NORMAL0;
};


struct CommonVSOutput
{
   float4 Position         : POSITION;
   float  LightingFactor   : TEXCOORD1;
};



// ==================== Textured data structures =======================
struct TexturedVSInput
{
   CommonVSInput  Common;
   float2         TexCoords      : TEXCOORD0;
};

struct TexturedVSOutput
{
   CommonVSOutput  Common;
   float2          TexCoords      : TEXCOORD0;
};
// ====================================================================

// ==================== Colored data structures =======================
struct ColoredVSInput
{
   float4         Position   : POSITION;
   float4         Color      : TEXCOORD1;
};

struct ColoredVSOutput
{
   float4          Position   : POSITION;
   float4          Color      : TEXCOORD1;
};
// ====================================================================

inline CommonVSOutput CommonVertexShader(CommonVSInput input)
{
   CommonVSOutput output;
   float4 worldPosition = mul(input.Position, xWorld);
   float4x4 preViewProjection = mul(xView, xProjection);

   output.Position = mul(worldPosition, preViewProjection);

   float3 Normal = mul(input.Normal, xWorld);
   float3 lightVector = normalize(LightSource - worldPosition);
   output.LightingFactor = saturate(dot(input.Normal, lightVector));

   return output;
}


// ============= Colored shaders  =====================================

float4 SolidColoredPixelShader(CommonVSOutput input) : COLOR0
{
   float4 color = xSolidColor;
   color.rgb *= (input.LightingFactor + AmbientLight);
   return color;
}


// ============= Textured shaders  =====================================
TexturedVSOutput TexturedVertexShader(TexturedVSInput input)
{
   TexturedVSOutput output;
   output.Common = CommonVertexShader(input.Common);
   output.TexCoords = input.TexCoords;
   return output;
}

float4 TexturedPixelShader(TexturedVSOutput input) : COLOR0
{
   float4 color = tex2D(TextureSampler, input.TexCoords);
   color.rgb *= (input.Common.LightingFactor + AmbientLight);
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
   return input.Color;
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
