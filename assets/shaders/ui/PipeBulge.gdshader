shader_type canvas_item;

// Basic parameters
uniform float token_progress : hint_range(0.0, 1.0) = 0.0;  // Position along the pipe (0-1)
uniform bool token_active = false;  // Whether to show the effect
uniform vec4 line_color : source_color = vec4(1.0);  // Color of the line

// Effect parameters
uniform float bulge_amount : hint_range(0.0, 10.0) = 3.0;  // How much the line bulges
uniform float bulge_softness : hint_range(0.01, 1.0) = 0.15;  // How soft/sharp the bulge is
uniform float base_width = 4.0;  // Base width of the line

void fragment() {
    // Get UV coordinates from Line2D
    float along_line = UV.x;  // UV.x goes from 0 to 1 along the line
    float across_line = UV.y; // UV.y goes from 0 to 1 across the line width
    
    // Calculate distance from token position, accounting for Line2D UV mapping
    float dist = abs(along_line - token_progress);
    
    // Calculate bulge effect with sharper falloff
    float bulge = 0.0;
    if (token_active) {
        // Exponential falloff for sharper effect
        bulge = exp(-dist * (1.0 / bulge_softness)) * bulge_amount;
        
        // Add a secondary pulse effect
        float pulse = sin(dist * 30.0 - TIME * 5.0) * 0.2;
        bulge += pulse * exp(-dist * 5.0);
    }
    
    // Calculate alpha based on vertical position with the bulge effect
    float width_factor = 1.0 + bulge * 0.5;
    float center_dist = abs(across_line - 0.5) / width_factor;
    
    // Sharper edge with smooth falloff
    float alpha = smoothstep(0.5, 0.4, center_dist);
    
    // Add glow effect when token is active
    float glow = 0.0;
    if (token_active) {
        glow = smoothstep(1.0, 0.0, dist * 5.0) * 0.5;
        glow *= smoothstep(0.8, 0.0, center_dist);
    }
    
    // Output final color with enhanced brightness for active token
    COLOR = line_color;
    COLOR.rgb *= 1.0 + glow * 2.0;  // Boost brightness for glow
    COLOR.a = alpha + glow;
    
    // Add highlight at token position
    if (token_active && dist < 0.05) {
        COLOR.rgb += vec3(0.5) * (1.0 - dist * 20.0);
    }
}
