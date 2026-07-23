#!/usr/bin/env python3
"""Shared medal-style framing for HypnosisCreator power icons."""

from __future__ import annotations

from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter


def center_square_crop(img: Image.Image) -> Image.Image:
    w, h = img.size
    side = min(w, h)
    left = (w - side) // 2
    top = (h - side) // 2
    return img.crop((left, top, left + side, top + side))


def _circle_mask(size: int, radius: float, *, antialias: int = 4) -> Image.Image:
    """Soft-edged circular alpha mask."""
    scale = antialias
    big = size * scale
    mask = Image.new("L", (big, big), 0)
    draw = ImageDraw.Draw(mask)
    inset = (big - int(radius * 2 * scale)) / 2
    draw.ellipse(
        (inset, inset, big - inset - 1, big - inset - 1),
        fill=255,
    )
    mask = mask.filter(ImageFilter.GaussianBlur(radius=max(1, scale // 2)))
    return mask.resize((size, size), Image.Resampling.LANCZOS)


def apply_medal_frame(content: Image.Image, size: int) -> Image.Image:
    """
    Fit content into a medal-like circular frame.
    Dark rim for dark UI (no bright sticker halo).
    """
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    pad = max(3, size // 18)
    inner = size - pad * 2

    art = content.convert("RGBA")
    art.thumbnail((inner, inner), Image.Resampling.LANCZOS)
    rgb = ImageEnhance.Contrast(art.convert("RGB")).enhance(1.05)
    alpha = art.split()[-1]
    art = Image.merge("RGBA", (*rgb.split(), alpha))

    ox = (size - art.width) // 2
    oy = (size - art.height) // 2
    layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    layer.alpha_composite(art, (ox, oy))

    inner_radius = inner / 2
    clip = _circle_mask(size, inner_radius)
    r, g, b, a = layer.split()
    layer = Image.merge("RGBA", (r, g, b, ImageChops.multiply(a, clip)))

    draw = ImageDraw.Draw(canvas)
    cx = cy = size / 2
    outer_r = inner_radius + pad * 0.35
    mid_r = inner_radius + pad * 0.12
    # Outer dark rim
    draw.ellipse(
        (cx - outer_r, cy - outer_r, cx + outer_r - 1, cy + outer_r - 1),
        outline=(34, 26, 22, 230),
        width=max(1, size // 32),
    )
    # Inner bronze highlight
    draw.ellipse(
        (cx - mid_r, cy - mid_r, cx + mid_r - 1, cy + mid_r - 1),
        outline=(92, 68, 42, 180),
        width=max(1, size // 48),
    )

    canvas.alpha_composite(layer)
    return canvas


def make_power_icon(content: Image.Image, size: int) -> Image.Image:
    """Public entry: square source art -> medal icon."""
    return apply_medal_frame(content, size)
