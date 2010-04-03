float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

Texture xTexture;   // используетс€ при отрисовке техникой Textured
float4 xSolidColor; // используетс€ при отрисовке техникой SolidColored

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
не повлил€ет ли на эффективность использование CommonVertexShader и соответствующих Common-структур?
¬озможно, в данном случае "дешевле" продублировать код.
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


// ====================================================================
technique SolidColored // ќтрисовка сплошным заданным цветом
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 CommonVertexShader();
        PixelShader = compile ps_2_0 SolidColoredPixelShader();
    }
}

technique Textured // ќтрисовка с наложением текстуры
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 TexturedVertexShader();
        PixelShader = compile ps_2_0 TexturedPixelShader();
    }
}
