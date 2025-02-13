shader_type canvas_item;

uniform vec2 token_position;
uniform vec2 curve_points[50];  // Fixed size array
uniform int point_count;
uniform float bulge_amount : hint_range(0.0, 2.0) = 1.5;  // Increased for larger effect
uniform float bulge_softness : hint_range(0.0, 1.0) = 0.15;  // Decreased for sharper edge
uniform vec4 line_color : source_color = vec4(1.0);  // Add color parameter

float get_distance_to_curve(vec2 point) {
    float min_dist = 1e10;
    vec2 closest_point = vec2(0.0);
    
    // Need at least 2 points to form a line
    if (point_count < 2) return min_dist;
    
    // Check distance to each line segment
    for (int i = 0; i < 49; i++) {  // Fixed size - 1
        if (i >= point_count - 1) break;  // Early exit if we've processed all points
        
        vec2 a = curve_points[i];
        vec2 b = curve_points[i + 1];
        
        // Line segment vector
        vec2 line_vec = b - a;
        float line_length = length(line_vec);
        
        if (line_length == 0.0) continue;
        
        // Normalized direction vector
        vec2 line_dir = line_vec / line_length;
        
        // Vector from point A to our test point
        vec2 point_vec = point - a;
        
        // Project point onto line segment
        float t = clamp(dot(point_vec, line_dir), 0.0, line_length);
        
        // Get closest point on line segment
        vec2 closest = a + line_dir * t;
        
        // Calculate distance
        float dist = length(point - closest);
        if (dist < min_dist) {
            min_dist = dist;
            closest_point = closest;
        }
    }
    
    // Calculate distance to token position projected onto curve
    float token_curve_dist = length(closest_point - token_position);
    
    return vec2(min_dist, token_curve_dist).x;  // Return the distance to curve
}

void fragment() {
    // Get distance from current pixel to curve
    vec2 pixel_pos = FRAGCOORD.xy;
    float dist = get_distance_to_curve(pixel_pos);
    
    // Calculate distance from token to pixel
    float token_dist = length(pixel_pos - token_position);
    
    // Create circular bulge effect
    float base_width = 4.0;  // Increased base pipe width
    float bulge_radius = 75.0;  // Significantly increased radius (5x from 15.0)
    float bulge_falloff = exp(-token_dist * bulge_softness);
    float bulge = bulge_amount * bulge_radius * bulge_falloff;
    
    // Calculate final width with smooth transition
    float width = base_width + bulge;
    
    // Calculate alpha with sharp edge for circular appearance
    float alpha = 1.0 - smoothstep(width - 0.5, width + 0.5, dist);
    
    // Output color using the uniform color
    COLOR = line_color * vec4(1.0, 1.0, 1.0, alpha);
}
