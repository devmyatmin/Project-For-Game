shader_type spatial;
// The generated heightmap mesh can have reversed triangle winding after
// Unity-to-Godot row conversion. Keep the terrain visible from both sides;
// vertex normals are explicitly generated to point upward.
render_mode cull_disabled;

uniform sampler2D grass_texture : source_color, repeat_enable;
uniform sampler2D sand_texture : source_color, repeat_enable;

varying float terrain_height;

void vertex() {
    terrain_height = VERTEX.y;
}

void fragment() {
    vec3 grass = texture(grass_texture, UV).rgb;
    vec3 sand = texture(sand_texture, UV).rgb;
    float beach_blend = 1.0 - smoothstep(0.25, 3.0, terrain_height);
    // Keep the imported ground texture natural under direct sunlight.
    // No metallic/specular shine: soil and grass should remain matte.
    ALBEDO = mix(grass, sand, beach_blend) * vec3(0.82, 0.82, 0.76);
    METALLIC = 0.0;
    ROUGHNESS = 1.0;
    SPECULAR = 0.12;
}
