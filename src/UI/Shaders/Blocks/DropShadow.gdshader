shader_type canvas_item;

uniform float blur_amount : hint_range(0.0, 5.0) = 2.0;
uniform float shadow_opacity : hint_range(0.0, 1.0) = 0.5;
uniform vec4 shadow_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void fragment() {
    vec2 ps = TEXTURE_PIXEL_SIZE;
    vec2 uv = UV;

    // Sample original texture alpha for shadow shape
    float a = texture(TEXTURE, uv).a;

    // Simple blur
    float blur = 0.0;
    float total = 0.0;

    for(float x = -blur_amount; x <= blur_amount; x += 1.0) {
        for(float y = -blur_amount; y <= blur_amount; y += 1.0) {
            vec2 coord = uv + vec2(x, y) * ps;
            float weight = exp(-(x*x + y*y)/(2.0 * blur_amount * blur_amount));
            blur += texture(TEXTURE, coord).a * weight;
            total += weight;
        }
    }

    // Normalize blur
    blur = blur / total;

    // Combine shadow color with blur and opacity
    COLOR = vec4(shadow_color.rgb, blur * shadow_opacity * a);
}