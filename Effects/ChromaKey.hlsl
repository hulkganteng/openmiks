sampler2D input : register(s0);
float4 keyColor : register(c0);
float similarity : register(c1);
float smoothness : register(c2);
float spillSuppression : register(c3);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv);
    if (color.a == 0.0) return color;

    // Euclidean distance in RGB color space
    float3 diff = color.rgb - keyColor.rgb;
    float dist = length(diff);

    // Smooth transition from keyed to unkeyed
    float edge0 = similarity;
    float edge1 = similarity + smoothness;
    float alpha = smoothstep(edge0, edge1, dist);

    // Green/Color Spill suppression
    if (spillSuppression > 0.0)
    {
        float maxOther = max(color.r, color.b);
        if (color.g > maxOther)
        {
            color.g = lerp(color.g, maxOther, spillSuppression);
        }
    }

    color.a *= alpha;
    return color;
}
