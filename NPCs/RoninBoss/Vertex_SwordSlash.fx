matrix WorldViewProjection;
float transparencyMultiplierX;
float transparencyMultiplierY;
float bottomOutlineBoundsPercent;
float upperOutlineBoundsPercent;
float outlineTransparencyMultiplier;
float slashProgress;
float endFadePercentStartPosition;
float endFadePercentStartTime;
float4 startColor;
float4 middleColor;
float4 endColor;
float waveFunctionExponent;

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float texCoordX = (input.TextureCoordinates.x);
    //float amountToMultiply = (1 + slashProgress);
    //if(amountToMultiply > 1)
    //    amountToMultiply = 1;
    //texCoordX *= 1 / amountToMultiply;
    //texCoordX = texCoordX;
    if(texCoordX < 0 || texCoordX > 1)
        return float4(0, 0, 0, 0);
    //texCoordX *= ;//will start reducing the thing when uhhhhhh swing is over I think so the cutoff is avoided
    //might have gotten it wrong and maybe will make the cutoff twice as worse uh oh
    float transparency = 0.5;
    //if(transparency > 1)
    //    transparency = 1;
    transparency *= 1 - pow(cos(texCoordX * 3.1415926), waveFunctionExponent); //1 - input.TextureCoordinates.x;
    if (input.TextureCoordinates.y < upperOutlineBoundsPercent || input.TextureCoordinates.y > bottomOutlineBoundsPercent)//give it some bright edges
    { 
        transparency *= outlineTransparencyMultiplier;
    }
    transparency = clamp(transparency, 0, 1);
    float t = texCoordX * 2;
    if(t < 1)
        return lerp(startColor * transparency, middleColor * transparency, t % 1);
    return lerp(middleColor * transparency, endColor * transparency, t % 1);
}


technique Technique1
{
    pass SlashTrailPass
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 MainPS();
    }
}