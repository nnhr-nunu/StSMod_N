shader_type canvas_item;

// グリーンバック透過（クロマキー）
// 調整は chroma_key_material.tres の shader_parameter をいじる

/// 抜く色（グリーンバックの代表色）
uniform vec4 key_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);

/// この距離以内の色を「背景」とみなす（大きいほど多く抜ける）
uniform float similarity : hint_range(0.0, 1.0, 0.001) = 0.40;

/// 境界のぼかし幅（大きいほど縁がなめらか／少し透ける）
uniform float smoothness : hint_range(0.0, 1.0, 0.001) = 0.08;

/// 緑の色かぶり抑制（髪・服の縁の緑残り対策）
uniform float spill : hint_range(0.0, 1.0, 0.001) = 0.20;

void fragment() {
	vec4 col = texture(TEXTURE, UV);
	float dist = distance(col.rgb, key_color.rgb);

	// similarity 未満 → 透明、smoothness でフェード
	float alpha_mask = smoothstep(similarity, similarity + smoothness, dist);

	// 緑かぶりを少し落とす（キャラ本体の緑はあまり触らない程度）
	float green_excess = max(col.g - max(col.r, col.b), 0.0);
	col.rgb -= vec3(0.0, green_excess * spill, 0.0);

	COLOR = vec4(col.rgb, col.a * alpha_mask);
}
