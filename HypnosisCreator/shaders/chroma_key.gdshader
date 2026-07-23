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

/// flip_h ON のとき画像右下ロゴは UV 右側（テクスチャ座標）。1.0 だと左下角を隠して効かない。
uniform float wm_on_uv_left : hint_range(0.0, 1.0, 1.0) = 0.0;

/// ロゴ文字検出の強さ（1＝既定。上げると白文字をもっと落とす）
uniform float wm_logo_strength : hint_range(0.0, 2.0, 0.01) = 1.0;

/// 敗北ディゾルブ進行度（0＝通常表示、1＝完全消滅）
uniform float dissolve_amount : hint_range(0.0, 1.0, 0.001) = 0.0;

/// ディゾルブ縁のぼかし幅
uniform float dissolve_edge : hint_range(0.01, 0.25, 0.001) = 0.07;

/// ディゾルブ縁の色（催眠っぽい淡い紫）
uniform vec4 dissolve_color : source_color = vec4(0.72, 0.45, 0.95, 0.85);

float hash21(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float value_noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	float a = hash21(i);
	float b = hash21(i + vec2(1.0, 0.0));
	float c = hash21(i + vec2(0.0, 1.0));
	float d = hash21(i + vec2(1.0, 1.0));
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

float fbm(vec2 p) {
	float v = 0.0;
	float amp = 0.5;
	for (int i = 0; i < 4; i++) {
		v += value_noise(p) * amp;
		p *= 2.05;
		amp *= 0.5;
	}
	return v;
}

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
	// strength=1 は旧既定（band 0.915〜0.948 / 左 0.38〜0.48）と一致。上げると帯と閾値を広げる。
	float boost = clamp(wm_logo_strength - 1.0, 0.0, 1.0);
	float logo_band = smoothstep(mix(0.915, 0.900, boost), mix(0.948, 0.960, boost), UV.y);
	float logo_x = mix(
		smoothstep(mix(0.52, 0.45, boost), mix(0.62, 0.58, boost), UV.x),
		1.0 - smoothstep(mix(0.38, 0.30, boost), mix(0.48, 0.55, boost), UV.x),
		wm_on_uv_left
	);
	float lum_th = mix(0.78, 0.70, boost);
	float sat_th = mix(0.16, 0.22, boost);
	float logo_flat = step(lum_th, lum) * step(sat, sat_th);
	float logo_on_green = step(0.12, green_dom) * step(0.72, lum) * step(sat, 0.20);
	float logo_kill = logo_band * logo_x * max(logo_flat, logo_on_green) * clamp(wm_logo_strength, 0.0, 2.0);
	alpha_mask *= 1.0 - clamp(logo_kill, 0.0, 1.0);

	// --- 敗北ディゾルブ（ノイズで欠けて消える） ---
	if (dissolve_amount > 0.001) {
		// 縦長立ち絵向けにやや縦伸びノイズ
		float n = fbm(UV * vec2(6.5, 9.5) + vec2(1.7, 0.3));
		float edge = max(dissolve_edge, 0.01);
		float cutoff = mix(-edge, 1.0 + edge, dissolve_amount);
		float keep = smoothstep(cutoff - edge, cutoff + edge * 0.15, n);
		float rim = 1.0 - smoothstep(0.0, edge, abs(n - cutoff));
		rim *= smoothstep(0.0, 0.15, dissolve_amount) * (1.0 - smoothstep(0.85, 1.0, dissolve_amount));
		col.rgb = mix(col.rgb, dissolve_color.rgb, rim * dissolve_color.a);
		alpha_mask *= keep;
	}

	COLOR = vec4(col.rgb, col.a * alpha_mask);
}
