shader_type canvas_item;

// Quantization settings
uniform float quantize_size : hint_range(0, 100, 1.0) = 20.0;
uniform bool handle_scale = true;
uniform float blur_amount : hint_range(0.0, 2.0) = 0.5;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;

// Pass necessary matrices to fragment shader
varying mat4 g_model_matrix;
varying mat4 g_world_to_clip;
varying vec2 g_vertex;
varying float g_q_size;

void vertex() {
    // Get the canvas matrix scale for proper quantization
    vec2 g_zoom = vec2(length(CANVAS_MATRIX[0].xyz), length(CANVAS_MATRIX[1].xyz));
    g_q_size = quantize_size;
    
    // Setup matrices for quantization
    g_model_matrix = MODEL_MATRIX;
    g_world_to_clip = SCREEN_MATRIX * CANVAS_MATRIX;
    g_vertex = VERTEX;
}

vec4 blur(sampler2D tex, vec2 uv, vec2 pixel_offset_size) {
    vec4 color = vec4(0.0);
    float total_weight = 0.0;
    
    for(float x = -2.0; x <= 2.0; x += 1.0) {
        for(float y = -2.0; y <= 2.0; y += 1.0) {
            vec2 offset = vec2(x, y) * pixel_offset_size * blur_amount;
            float weight = exp(-(x*x + y*y) / 8.0);
            color += textureLod(tex, uv + offset, 0.0) * weight;
            total_weight += weight;
        }
    }
    
    return color / total_weight;
}

void fragment() {
    vec4 screen_color;
    
    if (g_q_size == 0.0) {
        screen_color = texture(SCREEN_TEXTURE, SCREEN_UV);
    } else {
        // Convert to world space for quantization
        vec2 uv = (g_model_matrix * vec4(g_vertex, 0.0, 1.0)).xy;
        
        // Quantize to specified pixel size
        uv /= g_q_size;
        uv = floor(uv) + 0.5;
        uv *= g_q_size;
        
        // Convert back to screen space
        uv = (g_world_to_clip * vec4(uv, 0.0, 1.0)).xy;
        uv = uv * 0.5 + 0.5;
        
        // Apply blur to the quantized result
        screen_color = blur(SCREEN_TEXTURE, uv, SCREEN_PIXEL_SIZE);
        
        // Add a slight darkening effect
        screen_color.rgb *= 0.95;
    }
    
    COLOR = screen_color;
}
