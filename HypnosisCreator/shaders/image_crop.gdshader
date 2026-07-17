shader_type canvas_item;

// 表示側のパン／ズーム（画像ファイルは切らずに見た目だけ調整）
uniform vec2 offset = vec2(0.0);
uniform float zoom : hint_range(0.25, 4.0, 0.01) = 1.0;

void fragment() {
	float z = max(zoom, 0.01);
	vec2 uv = (UV - vec2(0.5)) / z + vec2(0.5) + offset;
	uv = clamp(uv, vec2(0.0), vec2(1.0));
	COLOR = texture(TEXTURE, uv);
}
