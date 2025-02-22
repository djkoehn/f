shader_type canvas_item;
#ifndef SCREEN_PIXEL_SIZE
	#define SCREEN_PIXEL_SIZE vec2(1.0/vec2(textureSize(SCREEN_TEXTURE, 0)))
#endif

// Controls
uniform float jpeg_quality : hint_range(0.0, 100.0, 1.0) = 100.0;
uniform float block_size : hint_range(1.0, 16.0, 1.0) = 8.0;
uniform float strobe_speed : hint_range(0.0, 2.0, 0.01) = 1.0; // Controls animation speed, 0 = static

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;

// Random function that takes a vec2 and returns a random float
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

// Dynamic noise function that changes per-frame
float dynamic_noise(vec2 uv, float time) {
    vec2 noise_uv = uv + time * 0.1 * strobe_speed;
    float noise = random(noise_uv);
    float noise2 = random(noise_uv * 2.0 + 1234.5678);
    return mix(noise, noise2, 0.5);
}

vec2 round_uv_to_block(vec2 uv) {
    return floor(uv / (SCREEN_PIXEL_SIZE * block_size)) * (SCREEN_PIXEL_SIZE * block_size);
}

// Convert RGB to YCbCr
vec3 rgb_to_ycbcr(vec3 rgb) {
    return vec3(
        0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b,
        -0.169 * rgb.r - 0.331 * rgb.g + 0.500 * rgb.b,
        0.500 * rgb.r - 0.419 * rgb.g - 0.081 * rgb.b
    );
}

// Convert YCbCr to RGB
vec3 ycbcr_to_rgb(vec3 ycbcr) {
    return vec3(
        ycbcr.x + 1.403 * ycbcr.z,
        ycbcr.x - 0.344 * ycbcr.y - 0.714 * ycbcr.z,
        ycbcr.x + 1.773 * ycbcr.y
    );
}

// Exponential noise function similar to more-jpeg
float exponential_noise(vec2 uv, float scale) {
    vec2 noise_uv = uv * scale;
    float noise = fract(sin(dot(noise_uv, vec2(12.9898, 78.233))) * 43758.5453);
    return noise * noise; // Make it exponential
}

vec4 jpeg_artifacts(sampler2D tex, vec2 uv, float quality, float time) {
    float compression = 1.0 - (quality / 100.0);
    compression = pow(compression, 1.5);
    
    vec4 base_color = texture(tex, uv);
    
    // Use uniform block_size parameter
    vec2 block_uv = floor(uv / (SCREEN_PIXEL_SIZE * block_size)) * (SCREEN_PIXEL_SIZE * block_size);
    vec2 within_block = fract(uv / (SCREEN_PIXEL_SIZE * block_size));
    
    // Add dynamic noise to the UV coordinates with strobe control
    vec2 noisy_uv = uv + (dynamic_noise(uv, time * strobe_speed) - 0.5) * SCREEN_PIXEL_SIZE * compression * 2.0;
    vec4 pixel_color = base_color;
    vec3 ycbcr = rgb_to_ycbcr(pixel_color.rgb);
    
    // Create horizontal scan-line effect with controlled time-based variation
    float scan_pos = within_block.y * block_size + time * 4.0 * strobe_speed;
    float scan_line = floor(scan_pos);
    float scan_offset = abs(fract(scan_pos) - 0.5) * 2.0;
    
    // Extreme quantization with controlled random variation
    float random_offset = (random(block_uv + time * strobe_speed) - 0.5) * 2.0;
    float y_steps = mix(256.0, 3.0 + random_offset, compression);
    float cb_steps = mix(256.0, 2.0, compression);
    float cr_steps = mix(256.0, 2.0, compression);
    
    // Quantize with scan-line and controlled noise influence
    float noise_factor = dynamic_noise(block_uv, time * strobe_speed) * compression * 0.1;
    ycbcr.x = floor((ycbcr.x + noise_factor) * y_steps + scan_offset * compression) / y_steps;
    
    // Sample neighboring blocks for color bleeding
    vec2 block_offset = SCREEN_PIXEL_SIZE * block_size;
    vec3 ycbcr_right = rgb_to_ycbcr(texture(tex, uv + vec2(block_offset.x, 0.0)).rgb);
    vec3 ycbcr_left = rgb_to_ycbcr(texture(tex, uv - vec2(block_offset.x, 0.0)).rgb);
    
    float luma_diff = abs(ycbcr.x - ycbcr_right.x) + abs(ycbcr.x - ycbcr_left.x);
    float bleed_strength = smoothstep(0.1, 0.3, luma_diff) * compression;
    
    if (luma_diff > 0.2) {
        vec2 random_shift = vec2(
            random(block_uv + time * 0.5 * strobe_speed),
            random(block_uv + time * 0.5 * strobe_speed + 1234.5678)
        );
        ycbcr.y += (random_shift.x - 0.5) * bleed_strength;
        ycbcr.z += (random_shift.y - 0.5) * bleed_strength;
    }
    
    float edge_factor = max(abs(within_block.x - 0.5), abs(within_block.y - 0.5)) * 2.0;
    float random_chroma = random(block_uv + vec2(time, time * 2.0) * strobe_speed);
    float chroma_steps = mix(cb_steps, cb_steps * 0.5, edge_factor * compression);
    ycbcr.yz = floor(ycbcr.yz * chroma_steps + (random_chroma - 0.5) * compression) / chroma_steps;
    
    vec3 final_color = ycbcr_to_rgb(ycbcr);
    
    float rgb_steps = mix(256.0, 4.0, compression * compression);
    final_color = floor(final_color * rgb_steps + dynamic_noise(uv, time * strobe_speed) * compression) / rgb_steps;
    
    if (luma_diff > 0.3 && compression > 0.5) {
        vec3 ring_color = vec3(1.0) - final_color;
        float ring_strength = dynamic_noise(uv + time * strobe_speed, time * strobe_speed) * compression * 0.3 * scan_offset;
        final_color = mix(final_color, ring_color, ring_strength);
    }
    
    vec3 noise_color = vec3(
        dynamic_noise(uv + vec2(0.0, time * strobe_speed), time * strobe_speed),
        dynamic_noise(uv + vec2(1.0, time * strobe_speed), time * strobe_speed),
        dynamic_noise(uv + vec2(2.0, time * strobe_speed), time * strobe_speed)
    ) - 0.5;
    final_color += noise_color * compression * 0.1;
    
    return vec4(clamp(final_color, 0.0, 1.0), pixel_color.a);
}

void fragment() {
    if (jpeg_quality >= 100.0) {
        COLOR = texture(SCREEN_TEXTURE, SCREEN_UV);
    } else {
        COLOR = jpeg_artifacts(SCREEN_TEXTURE, SCREEN_UV, jpeg_quality, TIME);
    }
}
