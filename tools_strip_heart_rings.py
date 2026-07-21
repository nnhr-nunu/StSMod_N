# -*- coding: utf-8 -*-
"""Remove red ring + dark disc fill from existing heart relic icons (post-process).

Matches the old tools_gen_heart_icons.make_icon palette:
  fill   ≈ (24, 12, 20, 230)
  outline ≈ (190, 50, 80, 255)
"""
from __future__ import annotations

import sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    import subprocess

    subprocess.check_call([sys.executable, "-m", "pip", "install", "pillow", "-q"])
    from PIL import Image

ROOT = Path(r"D:\Dev\antigravity\StSMod_N\HypnosisCreator\images\relics")
DIRS = [ROOT, ROOT / "big"]

FILL = (24, 12, 20)
RING = (190, 50, 80)


def near(c: tuple[int, int, int], t: tuple[int, int, int], tol: int) -> bool:
    return all(abs(a - b) <= tol for a, b in zip(c, t))


def strip(im: Image.Image) -> Image.Image:
    im = im.convert("RGBA")
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a < 8:
                continue
            rgb = (r, g, b)
            if near(rgb, RING, 18):
                px[x, y] = (0, 0, 0, 0)
                continue
            # dark disc fill (keep similar darks that belong to sprites by requiring high alpha match)
            if near(rgb, FILL, 10) and a >= 200:
                px[x, y] = (0, 0, 0, 0)
    return im


def main() -> None:
    n = 0
    for d in DIRS:
        for path in sorted(d.glob("*heart*.png")):
            if path.name.endswith("_outline.png"):
                continue
            before = path.stat().st_size
            out = strip(Image.open(path))
            out.save(path)
            n += 1
            print(f"stripped {path.relative_to(ROOT.parent.parent)} ({before}->{path.stat().st_size})")
    print(f"done n={n}")


if __name__ == "__main__":
    main()
