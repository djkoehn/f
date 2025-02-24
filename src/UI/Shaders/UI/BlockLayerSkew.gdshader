shader_type canvas_item;

uniform float fov : hint_range(0, 180) = 90.0;
uniform bool cull_back = true;
uniform float y_rot = 0.0;
uniform float x_rot = 0.0;
uniform float inset = 0.0;

// Lens effect parameters
uniform float jpeg_quality : hint_range(1, 100) = 10.0;
uniform float block_size : hint_range(1, 10) = 3.0;
uniform float strobe_speed : hint_range(0.0, 1.0) = 0.18;

void vertex() {
    // Apply perspective transformation
    float rad_fov = radians(fov);
    float half_fov = rad_fov / 2.0;
    float scale = 1.0 / tan(half_fov);
    
    vec3 pos = vec3(VERTEX.xy, 0.0);
    
    // Apply rotations
    float cos_y = cos(y_rot);
    float sin_y = sin(y_rot);
    float cos_x = cos(x_rot);
    float sin_x = sin(x_rot);
    
    vec3 rotated = vec3(
        pos.x * cos_y + pos.z * sin_y,
        pos.y * cos_x - pos.z * sin_x,
        -pos.x * sin_y + pos.z * cos_y
    );
    
    // Apply perspective projection
    float z = rotated.z + 2.0;
    vec2 projected = rotated.xy * scale / z;
    
    // Apply inset
    projected *= (1.0 - inset);
    
    VERTEX = projected;
}

void fragment() {
    // Get the base color
    vec4 base_color = texture(TEXTURE, UV);
    
    // Apply lens distortion effect
    vec2 block_uv = floor(UV * jpeg_quality) / jpeg_quality;
    vec2 block_center = (floor(UV * block_size) + 0.5) / block_size;
    float block_dist = length(UV - block_center);
    
    // Strobe effect
    float time_factor = TIME * strobe_speed;
    float strobe = sin(time_factor) * 0.5 + 0.5;
    
    // Mix the effects
    vec4 final_color = base_color;
    final_color.rgb += block_dist * strobe * 0.1;
    
    COLOR = final_color;
} 