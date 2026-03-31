shader_type spatial;

uniform vec4 albedo_color : source_color = vec4(0.8, 0.2, 0.2, 1.0);
uniform float flash_intensity : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    ALBEDO = mix(albedo_color.rgb, vec3(1.0), flash_intensity);
}
