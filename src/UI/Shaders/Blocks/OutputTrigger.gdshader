shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform float ripple_progress : hint_range(0.0, 1.0) = 0.0;
uniform float ripple_width : hint_range(0.0, 1.0) = 0.15;
uniform float fade_progress : hint_range(0.0, 1.0) = 0.0;
uniform float distortion_strength : hint_range(0.0, 1.0) = 0.0;
uniform vec2 center = vec2(0.5, 0.5);

const float RIPPLE_SOFTNESS = 1.0;

void fragment() {
    // Calculate aspect ratio corrected coordinates
    vec2 aspect_ratio = vec2(1.0, SCREEN_PIXEL_SIZE.x / SCREEN_PIXEL_SIZE.y);
    vec2 scaled_uv = (SCREEN_UV - vec2(0.5)) * aspect_ratio + vec2(0.5);
    vec2 scaled_center = (center - vec2(0.5)) * aspect_ratio + vec2(0.5);
    
    // Calculate distance from center in aspect-corrected space
    vec2 diff = scaled_uv - scaled_center;
    float dist = length(diff);
    
    // Calculate expanding ripple effect with constant softness
    float ripple_radius = ripple_progress * 2.0; // Double the expansion range
    float ripple_mask = smoothstep(ripple_radius - ripple_width - RIPPLE_SOFTNESS, 
                                 ripple_radius - ripple_width, 
                                 dist) *
                       (1.0 - smoothstep(ripple_radius, 
                                       ripple_radius + RIPPLE_SOFTNESS, 
                                       dist));
    
    // Apply fade out to the mask
    ripple_mask *= (1.0 - fade_progress);
    
    // Calculate distortion offset with separate strength control
    vec2 distortion_offset = normalize(diff) * ripple_mask * distortion_strength * sin(dist * 20.0 - ripple_progress * 10.0);
    
    // Sample the screen texture with distortion
    COLOR = texture(SCREEN_TEXTURE, SCREEN_UV + distortion_offset);
} 