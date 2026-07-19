shader_type canvas_item;

// グリーンバック透過（クロマキー）
// 調整は chroma_key_material.tres / Mod設定 の shader_parameter をいじる

/// 抜く色（グリーンバックの代表色）
uniform vec4 key_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);

/// この距離以内の色を「背景」とみなす（大きいほど多く抜ける）
uniform float similarity : hint_range(0.0, 1.0, 0.001) = 0.18;

/// 境界のぼかし幅（大きいほど縁がなめらか／少し透ける）
uniform float smoothness : hint_range(0.0, 1.0, 0.001) = 0.06;

/// 緑の色かぶり抑制（髪・服の縁の緑残り対策）
uniform float spill : hint_range(0.0, 1.0, 0.001) = 0.18;

/// 明るい粒子の色かぶりを白へ戻す量（Gだけ落とすと青っぽく残る対策）
uniform float sparkle_whiten : hint_range(0.0, 1.0, 0.001) = 0.9;

/// 下端から何割をウォーターマーク帯にするか（ハード角抜き）
uniform float wm_crop_bottom : hint_range(0.0, 0.25, 0.001) = 0.055;

/// ウォーターマーク側の横幅（UV）— 靴を避けるため控えめ
uniform float wm_crop_side : hint_range(0.0, 0.8, 0.001) = 0.38;

/// flip_h ON のとき画像右下ロゴは UV 左側に来る → 1.0
uniform float wm_on_uv_left : hint_range(0.0, 1.0, 1.0) = 1.0;

void fragment() {
	vec4 col = texture(TEXTURE, UV);
	float dist = distance(col.rgb, key_color.rgb);

	// similarity 未満 → 透明、smoothness でフェード
	float alpha_mask = smoothstep(similarity, similarity + smoothness, dist);

	float lum = dot(col.rgb, vec3(0.299, 0.587, 0.114));
	float green_dom = col.g - max(col.r, col.b);
	float sat = max(max(col.r, col.g), col.b) - min(min(col.r, col.g), col.b);

	// 白い靴・服を守る（明るい＆緑優勢でない画素は抜けにくくする）
	float protect = step(0.42, lum) * step(green_dom, 0.10);
	alpha_mask = mix(alpha_mask, 1.0, protect * 0.92);

	// 緑かぶり抑制（保護画素は弱める）
	float green_excess = max(green_dom, 0.0);
	col.rgb -= vec3(0.0, green_excess * spill * (1.0 - protect * 0.7), 0.0);

	// 明るい粒子: 緑を引いただけだと B が残り青〜シアンに見える → 輝度の白へ寄せる
	float cool_excess = max(max(col.b, col.g) - col.r, 0.0);
	float sparkle = smoothstep(0.52, 0.72, lum) * (1.0 - smoothstep(0.18, 0.42, sat));
	float whiten_w = sparkle * clamp(green_excess * 5.0 + cool_excess * 3.0, 0.0, 1.0) * sparkle_whiten;
	col.rgb = mix(col.rgb, vec3(lum), whiten_w);

	// --- ウォーターマーク ---
	// 1) 角のハード抜き（靴と重なりにくい外側だけ）
	float in_bottom = step(1.0 - wm_crop_bottom, UV.y);
	float in_side = mix(
		step(1.0 - wm_crop_side, UV.x),
		step(UV.x, wm_crop_side),
		wm_on_uv_left
	);
	alpha_mask *= 1.0 - in_bottom * in_side;

	// 2) 足元ロゴ帯の「白い文字」だけ落とす（靴は中央寄りなので角ゾーンに限定）
	// ロゴ文字: 明るい・ほぼ無彩度。靴の白い面は彩度や位置で残りやすい
	float logo_band = smoothstep(0.915, 0.948, UV.y);
	// ハード角より少し内側まで文字が伸びるので、そこだけ追加で落とす
	float logo_x = mix(
		smoothstep(0.52, 0.62, UV.x),
		1.0 - smoothstep(0.38, 0.48, UV.x),
		wm_on_uv_left
	);
	float logo_flat = step(0.78, lum) * step(sat, 0.16);
	float logo_on_green = step(0.12, green_dom) * step(0.72, lum) * step(sat, 0.20);
	float logo_kill = logo_band * logo_x * max(logo_flat, logo_on_green);
	alpha_mask *= 1.0 - logo_kill;

	COLOR = vec4(col.rgb, col.a * alpha_mask);
}
