shader_type canvas_item;

uniform vec4 fire_color : source_color = vec4(1.0, 0.78, 0.48, 1.0);
uniform float warmth_strength : hint_range(0.0, 1.0) = 0.32;
uniform float flicker_speed = 2.4;
uniform float fire_from_left : hint_range(0.0, 1.0) = 1.0;

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	if (tex.a < 0.01) {
		COLOR = tex;
		return;
	}

	float fire_side = mix(UV.x, 1.0 - UV.x, fire_from_left);
	float edge = smoothstep(0.15, 0.95, fire_side);
	float flicker = 0.88 + 0.12 * sin(TIME * flicker_speed + UV.y * 5.5);
	vec3 warm = tex.rgb * mix(vec3(1.0), fire_color.rgb, edge * warmth_strength * flicker);
	vec3 cooled = tex.rgb * mix(vec3(0.82, 0.86, 0.95), vec3(1.0), 1.0 - edge * 0.25);
	COLOR = vec4(mix(cooled, warm, 0.72), tex.a);
}
