shader_type canvas_item;

uniform vec4 color1 : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform vec4 color2 : source_color = vec4(0.8, 0.1, 0.1, 1.0);
uniform float time_scale : hint_range(0.01, 1.0) = 0.1;
uniform float distortion_strength : hint_range(0.0, 1.0) = 0.5;
uniform float interlace_size : hint_range(10.0, 100.0) = 40.0;
uniform float interlace_strength : hint_range(0.0, 1.0) = 0.5;
uniform float pattern_scale : hint_range(1.0, 50.0) = 20.0;
uniform float dither_size : hint_range(1.0, 10.0) = 2.0;
uniform vec2 scroll_speed = vec2(0.1, 0.1);

// Atkinson dither matrix
const mat4 dither_matrix = mat4(
    vec4(0.0, 0.5, 0.125, 0.625),
    vec4(0.75, 0.25, 0.875, 0.375),
    vec4(0.1875, 0.6875, 0.0625, 0.5625),
    vec4(0.9375, 0.4375, 0.8125, 0.3125)
);

float rand(vec2 coord) {
    return fract(sin(dot(coord, vec2(12.9898, 78.233))) * 43758.5453123);
}

float get_dither_value(vec2 pos) {
    ivec2 dither_pos = ivec2(mod(pos, 4.0));
    return dither_matrix[dither_pos.x][dither_pos.y];
}

void fragment() {
    // Base UV with mechanical-looking scrolling
    vec2 base_uv = UV + TIME * scroll_speed * time_scale;
    base_uv.x += sin(base_uv.y * pattern_scale + TIME) * 0.02;

    // Create mechanical distortion effect
    float distortion = sin(base_uv.x * pattern_scale + TIME) *
                      sin(base_uv.y * pattern_scale * 0.5 + TIME * 0.5);
    distortion *= distortion_strength;

    // Add horizontal interlacing effect
    float interlace = step(fract(UV.y * interlace_size), 0.5) * 2.0 - 1.0;
    interlace *= interlace_strength;

    // Apply both distortion and interlacing
    vec2 distorted_uv = base_uv + vec2(distortion + interlace * 0.1, distortion * 0.5);

    // Create mechanical pattern
    float pattern = sin(distorted_uv.x * pattern_scale) *
                   sin(distorted_uv.y * pattern_scale * 0.5);
    pattern = smoothstep(-1.0, 1.0, pattern);

    // Apply dithering
    vec2 dither_uv = FRAGCOORD.xy / dither_size;
    float dither = get_dither_value(dither_uv);
    pattern = step(dither, pattern);

    // Mix colors with mechanical feel
    vec4 final_color = mix(color1, color2, pattern);

    COLOR = final_color;
}