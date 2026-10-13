namespace CatVideo;

internal readonly record struct CatColorSettings(int Strength, int Brightness, int Contrast)
{
    public static CatColorSettings Default => new(75, 100, 100);

    public ColorEffect CreateEffect()
    {
        var strength = Math.Clamp(Strength, 0, 100) / 100f;
        var brightness = Math.Clamp(Brightness, 80, 120) / 100f;
        var contrast = Math.Clamp(Contrast, 80, 120) / 100f;

        // A heuristic recoloring for cats, NOT a simulation of what cats see.
        // At full strength: R' = G' = .20 R + .80 G; B' = .65 R + .35 B.
        // Red cues shift toward blue; green cues toward yellow. White and black
        // remain neutral before brightness/contrast. Some colors still collapse.
        // These are design coefficients, not measured feline cone sensitivities.
        float[,] recolor =
        {
            { .20f, .20f, .65f }, // Input red contributions to output R, G, B.
            { .80f, .80f, .00f }, // Input green.
            { .00f, .00f, .35f }  // Input blue.
        };

        var values = new float[25];
        for (var input = 0; input < 3; input++)
        {
            for (var output = 0; output < 3; output++)
            {
                var identity = input == output ? 1f : 0f;
                values[input * 5 + output] =
                    (identity * (1f - strength) + recolor[input, output] * strength)
                    * contrast * brightness;
            }
        }

        // Contrast around mid-gray, then apply the software brightness gain.
        var offset = .5f * (1f - contrast) * brightness;
        values[20] = values[21] = values[22] = offset;
        values[18] = values[24] = 1f;
        return new ColorEffect { Values = values };
    }
}
