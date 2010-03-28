float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

float3 LightSource = float3(10, 10, 10);
float AmbientLight = 0.15;


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

// ==================== Colored data structures =======================
struct ColoredVSInput
{
   CommonVSInput  Common;
   float4         Color      : COLOR0;
};

struct ColoredVSOutput
{
   CommonVSOutput    Common;
   float4            Color   : COLOR0;
};
// ====================================================================


// ==================== Textured data structures =======================
struct TexturedVSInput
{
   CommonVSInput  Common;
   float2         TexCoords      : TEXCOORD2;
};

struct TexturedVSOutput
{
   CommonVSOutput  Common;
   float2          TexCoords      : TEXCOORD2;
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
ColoredVSOutput ColoredVertexShader(ColoredVSInput input)
{
   ColoredVSOutput output;
   output.Common = CommonVertexShader(input.Common);
   output.Color = input.Color;
   return output;
}

float4 ColoredPixelShader(ColoredVSOutput input) : COLOR0
{
   float4 color = input.Color;
   color.rgb *= (input.Common.LightingFactor + AmbientLight);
   return color;
}


// ====================================================================
technique Colored
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 ColoredVertexShader();
        PixelShader = compile ps_2_0 ColoredPixelShader();
    }
}

