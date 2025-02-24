shader_type canvas_item;

uniform float glow_width : hint_range(0.0, 1.0) = 0.5;
uniform float glow_intensity : hint_range(0.0, 2.0) = 1.0;
uniform vec4 glow_color : source_color = vec4(1.0, 0.843, 0.0, 1.0);

void fragment() {
    vec4 base_color = texture(TEXTURE, UV);
    
    // Calculate distance from edge
    float alpha = base_color.a;
    float glow = smoothstep(1.0 - glow_width, 1.0, alpha) * glow_intensity;
    
    // Mix base color with glow
    vec4 final_color = mix(base_color, glow_color, glow);
    final_color.a = max(base_color.a, glow * 0.5);
    
    COLOR = final_color;
} 