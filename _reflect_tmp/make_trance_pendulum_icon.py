from pathlib import Path

from PIL import Image, ImageDraw, ImageEnhance, ImageFilter

src_path = Path(
    r"C:\Users\homut\.cursor\projects\d-Dev-antigravity-StSMod-N\assets\trance_pendulum_source.png"
)
out_small = Path(r"D:\Dev\antigravity\StSMod_N\HypnosisCreator\images\powers\trance_power.png")
out_big = Path(r"D:\Dev\antigravity\StSMod_N\HypnosisCreator\images\powers\big\trance_power.png")

src = Image.open(src_path).convert("RGBA")

# Near-black / near-white fringe to transparent
pixels = src.load()
w, h = src.size
for y in range(h):
    for x in range(w):
        r, g, b, a = pixels[x, y]
        if a == 0:
            continue
        # solid black background from generator
        if r < 28 and g < 28 and b < 28:
            pixels[x, y] = (0, 0, 0, 0)
            continue
        # crush near-opaque dark purple vignette leftovers into soft alpha later
        if r < 45 and g < 35 and b < 55 and max(r, g, b) < 60:
            pixels[x, y] = (0, 0, 0, 0)

bbox = src.getbbox()
if not bbox:
    raise SystemExit("no content")
src = src.crop(bbox)


def make_power_icon(content: Image.Image, size: int) -> Image.Image:
    """Power icons must keep true alpha. Opaque rectangular fill shows as a box in UI."""
    pad = max(4, size // 14)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))

    # Soft purple glow (also transparent outside the disc)
    glow = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    gd = ImageDraw.Draw(glow)
    m = size // 8
    gd.ellipse((m, m, size - 1 - m, size - 1 - m), fill=(120, 70, 180, 90))
    glow = glow.filter(ImageFilter.GaussianBlur(radius=max(2, size // 16)))
    canvas = Image.alpha_composite(canvas, glow)

    fit = size - pad * 2
    img = content.copy()
    img.thumbnail((fit, fit), Image.Resampling.LANCZOS)
    rgb = ImageEnhance.Contrast(img.convert("RGB")).enhance(1.12)
    a = img.split()[-1]
    img = Image.merge("RGBA", (*rgb.split(), a))

    ox = (size - img.width) // 2
    oy = (size - img.height) // 2
    canvas.alpha_composite(img, (ox, oy))
    return canvas


big = make_power_icon(src, 256)
small = make_power_icon(src, 64)
out_big.parent.mkdir(parents=True, exist_ok=True)
big.save(out_big)
small.save(out_small)

for label, im in (("big", big), ("small", small)):
    a = im.split()[-1]
    hist = a.histogram()
    total = im.size[0] * im.size[1]
    print(f"{label}: alpha=({a.getextrema()}) transparent={hist[0]}/{total} opaque={hist[255]}/{total}")
print("saved", out_big)
print("saved", out_small)
