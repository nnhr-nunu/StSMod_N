# -*- coding: utf-8 -*-
"""Convert numbered originals in card_portraits/original/ into game-ready PNGs.

Workflow:
  1. Place source art as `{CSV番号}.jpg|.png|.webp` under original/
  2. Run: python tools_process_card_portraits.py
  3. Outputs:
       card_portraits/{id}.png       (250x190)
       card_portraits/big/{id}.png   (1000x760)

CSV番号 → 日本語名 → localization/jpn/cards.json の title → ファイル名(snake)
"""
from __future__ import annotations

import argparse
import csv
import re
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent
PORTRAIT_DIR = ROOT / "HypnosisCreator" / "images" / "card_portraits"
ORIGINAL_DIR = PORTRAIT_DIR / "original"
BIG_DIR = PORTRAIT_DIR / "big"
JPN_CARDS = ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json"

# StS2 card portrait aspect (same as 250x190 / 1000x760)
ASPECT_W, ASPECT_H = 25, 19
SIZE_SMALL = (250, 190)
SIZE_BIG = (1000, 760)

IMAGE_EXTS = {".png", ".jpg", ".jpeg", ".webp", ".bmp", ".gif", ".tif", ".tiff"}

# CSV No → 追加で同じアートを書き出す entry（調教コマンド → 生成命令カード）
SHARED_EXTRA_ENTRIES: dict[int, tuple[str, ...]] = {
    48: (
        "kneel",
        "look",
        "come",
        "relax",
        "present",
        "trance",
        "crawl",
        "dont_move",
        "roll",
        "cum",
        "good",
    ),
}


def load_title_to_entry() -> dict[str, str]:
    import json

    data = json.loads(JPN_CARDS.read_text(encoding="utf-8"))
    out: dict[str, str] = {}
    for key, title in data.items():
        if not key.endswith(".title"):
            continue
        # HYPNOSISCREATOR-FINGER_SNAP.title → finger_snap
        entry = key.removeprefix("HYPNOSISCREATOR-").removesuffix(".title")
        out[str(title).strip()] = entry.lower()
    return out


def load_no_to_title(csv_path: Path) -> dict[int, str]:
    text = csv_path.read_text(encoding="utf-8-sig")
    reader = csv.DictReader(text.splitlines())
    if reader.fieldnames is None:
        raise SystemExit(f"CSV has no header: {csv_path}")

    name_col = None
    for col in reader.fieldnames:
        if "カード" in col or "名称" in col:
            name_col = col
            break
    if name_col is None:
        # fallback: 2nd column
        name_col = reader.fieldnames[1]

    out: dict[int, str] = {}
    for row in reader:
        no_raw = (row.get("No") or "").strip()
        if not no_raw.isdigit():
            continue
        title = (row.get(name_col) or "").strip().replace("\r", "")
        if not title:
            continue
        out[int(no_raw)] = title
    return out


def center_crop_to_aspect(img: Image.Image, aw: int = ASPECT_W, ah: int = ASPECT_H) -> Image.Image:
    w, h = img.size
    target = aw / ah
    current = w / h
    if abs(current - target) < 1e-6:
        return img
    if current > target:
        # too wide → crop sides
        new_w = int(round(h * target))
        left = (w - new_w) // 2
        return img.crop((left, 0, left + new_w, h))
    # too tall → crop top/bottom
    new_h = int(round(w / target))
    top = (h - new_h) // 2
    return img.crop((0, top, w, top + new_h))


def open_image(path: Path) -> Image.Image:
    img = Image.open(path)
    # Handle mislabeled extensions (e.g. WebP saved as .jpg)
    img.load()
    if img.mode not in ("RGB", "RGBA"):
        img = img.convert("RGBA" if "A" in img.getbands() else "RGB")
    elif img.mode == "RGBA":
        # Composite onto dark neutral so JPG/WebP transparency doesn't become white noise
        bg = Image.new("RGBA", img.size, (20, 16, 28, 255))
        img = Image.alpha_composite(bg, img)
    return img.convert("RGB")


def save_pair(small: Image.Image, big: Image.Image, entry: str, dry_run: bool = False) -> None:
    out_small = PORTRAIT_DIR / f"{entry}.png"
    out_big = BIG_DIR / f"{entry}.png"
    print(f"    → {entry}.png / big/{entry}.png")
    if dry_run:
        return
    BIG_DIR.mkdir(parents=True, exist_ok=True)
    small.save(out_small, format="PNG", optimize=True)
    big.save(out_big, format="PNG", optimize=True)


def process_one(src: Path, entry: str, extras: tuple[str, ...] = (), dry_run: bool = False) -> None:
    img = open_image(src)
    cropped = center_crop_to_aspect(img)
    big = cropped.resize(SIZE_BIG, Image.Resampling.LANCZOS)
    small = cropped.resize(SIZE_SMALL, Image.Resampling.LANCZOS)

    print(f"  {src.name} ({img.size[0]}x{img.size[1]}) → {entry}.png")
    print(f"    small {SIZE_SMALL[0]}x{SIZE_SMALL[1]}  big {SIZE_BIG[0]}x{SIZE_BIG[1]}")
    save_pair(small, big, entry, dry_run=dry_run)
    for extra in extras:
        if extra == entry:
            continue
        save_pair(small, big, extra, dry_run=dry_run)


def discover_numbered(original_dir: Path) -> list[tuple[int, Path]]:
    found: list[tuple[int, Path]] = []
    for path in sorted(original_dir.iterdir()):
        if not path.is_file():
            continue
        if path.suffix.lower() not in IMAGE_EXTS:
            continue
        m = re.fullmatch(r"(\d+)", path.stem)
        if not m:
            print(f"  skip (not numbered): {path.name}")
            continue
        found.append((int(m.group(1)), path))
    return found


def main() -> int:
    parser = argparse.ArgumentParser(description="Process numbered card portrait originals")
    parser.add_argument(
        "--csv",
        type=Path,
        default=Path(r"c:\Users\homut\Downloads\ヒプノクリエイター.csv"),
        help="CSV with No + Japanese card name",
    )
    parser.add_argument("--original", type=Path, default=ORIGINAL_DIR)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument(
        "--only",
        type=str,
        default="",
        help="Comma-separated CSV numbers to process (default: all in original/)",
    )
    args = parser.parse_args()

    if not args.csv.is_file():
        print(f"CSV not found: {args.csv}", file=sys.stderr)
        return 1
    if not args.original.is_dir():
        print(f"original dir not found: {args.original}", file=sys.stderr)
        return 1

    no_to_title = load_no_to_title(args.csv)
    title_to_entry = load_title_to_entry()
    only = {int(x) for x in args.only.split(",") if x.strip().isdigit()} if args.only else None

    items = discover_numbered(args.original)
    if not items:
        print("No numbered images in original/")
        return 0

    ok = 0
    errors: list[str] = []
    for no, path in items:
        if only is not None and no not in only:
            continue
        title = no_to_title.get(no)
        if not title:
            errors.append(f"No.{no}: CSVに名称なし ({path.name})")
            continue
        entry = title_to_entry.get(title)
        if not entry:
            errors.append(f"No.{no}「{title}」: cards.json に title 一致なし")
            continue
        try:
            extras = SHARED_EXTRA_ENTRIES.get(no, ())
            process_one(path, entry, extras=extras, dry_run=args.dry_run)
            ok += 1
        except Exception as exc:  # noqa: BLE001 — report per-file and continue
            errors.append(f"No.{no} ({path.name}): {exc}")

    print(f"\nDone: {ok} processed, {len(errors)} errors")
    for e in errors:
        print(f"  ERROR: {e}", file=sys.stderr)
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
