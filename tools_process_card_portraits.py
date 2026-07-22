# -*- coding: utf-8 -*-
"""Convert numbered originals in card_portraits/original/ into game-ready PNGs.

Workflow:
  1. Place source art as `{CSV番号}.jpg|.png|.webp` under original/
  2. Run: python tools_process_card_portraits.py
  3. Outputs:
       card_portraits/{id}.png       (250x190)
       card_portraits/big/{id}.png   (1000x760)

CSV番号 → 日本語名 → localization/jpn/cards.json の title → ファイル名(snake)

クロップ: 顔検出 → 没入度マップ（エッジ＋上部寄り）→ 中央。顔はカード枠のやや上寄りに配置。
"""
from __future__ import annotations

import argparse
import csv
import re
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parent
PORTRAIT_DIR = ROOT / "HypnosisCreator" / "images" / "card_portraits"
ORIGINAL_DIR = PORTRAIT_DIR / "original"
BIG_DIR = PORTRAIT_DIR / "big"
JPN_CARDS = ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json"

# StS2 card portrait aspect (same as 250x190 / 1000x760)
ASPECT_W, ASPECT_H = 25, 19
ASPECT = ASPECT_W / ASPECT_H
SIZE_SMALL = (250, 190)
SIZE_BIG = (1000, 760)

# 顔・主題をカード枠内のこの位置付近へ（上寄りが見栄えしやすい）
FOCAL_TARGET_X = 0.50
FOCAL_TARGET_Y = 0.38

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

def _sobel_magnitude(gray: np.ndarray) -> np.ndarray:
    gx = np.zeros_like(gray, dtype=np.float32)
    gy = np.zeros_like(gray, dtype=np.float32)
    gx[:, 1:-1] = gray[:, 2:] - gray[:, :-2]
    gy[1:-1, :] = gray[2:, :] - gray[:-2, :]
    return np.hypot(gx, gy)


def _gaussian_blur(gray: np.ndarray, radius: int = 8) -> np.ndarray:
    pil = Image.fromarray(gray.astype(np.uint8))
    blurred = pil.filter(ImageFilter.GaussianBlur(radius=radius))
    return np.array(blurred, dtype=np.float32)


def _saliency_focal(img_rgb: np.ndarray) -> tuple[float, float]:
    """Edge + contrast saliency with portrait-friendly top-center bias."""
    h, w = img_rgb.shape[:2]
    gray = np.dot(img_rgb[..., :3], [0.299, 0.587, 0.114]).astype(np.float32)

    edges = _sobel_magnitude(gray)
    blur = _gaussian_blur(gray, radius=max(4, min(w, h) // 120))
    contrast = np.abs(gray - blur)

    yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
    # 上部・中央をやや優遇（立ち絵・バストアップ向け）
    pos_w = np.exp(-((xx / max(w - 1, 1) - 0.5) ** 2) / (2 * 0.28**2))
    pos_w *= 0.50 + 0.50 * (1.0 - yy / max(h - 1, 1)) ** 0.85

    # 彩度が高い領域（キャラ・表情）を少し加点
    cmax = img_rgb.max(axis=2).astype(np.float32)
    cmin = img_rgb.min(axis=2).astype(np.float32)
    sat = (cmax - cmin) / (cmax + 1e-3)

    score = (edges * 0.45 + contrast * 0.35 + sat * 80.0 * 0.20) * pos_w

    step = max(8, min(w, h) // 48)
    score_ds = score[::step, ::step]
    yy_ds = yy[::step, ::step]
    xx_ds = xx[::step, ::step]
    total = float(score_ds.sum()) + 1e-6
    fx = float((xx_ds * score_ds).sum() / total)
    fy = float((yy_ds * score_ds).sum() / total)
    return fx / max(w - 1, 1), fy / max(h - 1, 1)


def focal_point(img: Image.Image) -> tuple[float, float, str]:
    """Return normalized focal (0-1), (0-1) and method label."""
    rgb = np.array(img)
    fx, fy = _saliency_focal(rgb)
    return fx, fy, "smart"


def load_title_to_entry() -> dict[str, str]:
    import json

    data = json.loads(JPN_CARDS.read_text(encoding="utf-8"))
    out: dict[str, str] = {}
    for key, title in data.items():
        if not key.endswith(".title"):
            continue
        entry = key.removeprefix("HYPNOSISCREATOR-").removesuffix(".title")
        out[str(title).strip()] = entry.lower()
    return out


def default_csv_path() -> Path:
    candidates = [
        Path(r"c:\Users\homut\Downloads\ヒプノクリエイター.csv"),
        ROOT / "_csv_full_latest.json",
    ]
    downloads = Path(r"c:\Users\homut\Downloads")
    if downloads.is_dir():
        dated = sorted(downloads.glob("*2026*.csv"), key=lambda p: p.stat().st_mtime, reverse=True)
        candidates = list(dated) + candidates
    for path in candidates:
        if path.is_file():
            return path
    return candidates[0]


def load_no_to_title(csv_path: Path) -> dict[int, str]:
    if csv_path.suffix.lower() == ".json":
        import json

        rows = json.loads(csv_path.read_text(encoding="utf-8"))
        name_col = "カード名称（日本語）"
        out: dict[int, str] = {}
        for row in rows:
            no_raw = str(row.get("No", "")).strip()
            if not no_raw.isdigit():
                continue
            title = str(row.get(name_col, "")).strip().replace("\r", "")
            if title:
                out[int(no_raw)] = title
        return out

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


def open_image(path: Path) -> Image.Image:
    img = Image.open(path)
    img.load()
    if img.mode not in ("RGB", "RGBA"):
        img = img.convert("RGBA" if "A" in img.getbands() else "RGB")
    elif img.mode == "RGBA":
        bg = Image.new("RGBA", img.size, (20, 16, 28, 255))
        img = Image.alpha_composite(bg, img)
    return img.convert("RGB")


def crop_to_aspect(img: Image.Image, fx: float, fy: float) -> Image.Image:
    w, h = img.size
    current = w / h

    if abs(current - ASPECT) < 1e-6:
        return img

    if current > ASPECT:
        crop_w = int(round(h * ASPECT))
        crop_h = h
    else:
        crop_w = w
        crop_h = int(round(w / ASPECT))

    # 焦点を FOCAL_TARGET 付近へ
    left = int(round(fx * w - FOCAL_TARGET_X * crop_w))
    top = int(round(fy * h - FOCAL_TARGET_Y * crop_h))

    left = max(0, min(left, w - crop_w))
    top = max(0, min(top, h - crop_h))
    return img.crop((left, top, left + crop_w, top + crop_h))


def save_pair(small: Image.Image, big: Image.Image, entry: str, dry_run: bool = False) -> None:
    out_small = PORTRAIT_DIR / f"{entry}.png"
    out_big = BIG_DIR / f"{entry}.png"
    print(f"    → {entry}.png / big/{entry}.png")
    if dry_run:
        return
    BIG_DIR.mkdir(parents=True, exist_ok=True)
    small.save(out_small, format="PNG", optimize=True)
    big.save(out_big, format="PNG", optimize=True)


def process_one(
    src: Path,
    entry: str,
    extras: tuple[str, ...] = (),
    dry_run: bool = False,
) -> str:
    img = open_image(src)
    fx, fy, method = focal_point(img)
    cropped = crop_to_aspect(img, fx, fy)
    big = cropped.resize(SIZE_BIG, Image.Resampling.LANCZOS)
    small = cropped.resize(SIZE_SMALL, Image.Resampling.LANCZOS)

    print(
        f"  {src.name} ({img.size[0]}x{img.size[1]}) → {entry}.png "
        f"[{method} @ ({fx:.2f},{fy:.2f})]"
    )
    print(f"    small {SIZE_SMALL[0]}x{SIZE_SMALL[1]}  big {SIZE_BIG[0]}x{SIZE_BIG[1]}")
    save_pair(small, big, entry, dry_run=dry_run)
    for extra in extras:
        if extra == entry:
            continue
        save_pair(small, big, extra, dry_run=dry_run)
    return method


def discover_numbered(
    original_dir: Path,
    *,
    on_duplicate: str = "error",
) -> list[tuple[int, Path]]:
    """One file per CSV No. on_duplicate: error | largest."""
    by_no: dict[int, list[Path]] = {}
    for path in original_dir.iterdir():
        if not path.is_file():
            continue
        if path.suffix.lower() not in IMAGE_EXTS:
            continue
        m = re.fullmatch(r"(\d+)", path.stem)
        if not m:
            print(f"  skip (not numbered): {path.name}")
            continue
        by_no.setdefault(int(m.group(1)), []).append(path)

    dup_errors: list[str] = []
    out: list[tuple[int, Path]] = []
    for no, paths in sorted(by_no.items()):
        if len(paths) > 1:
            names = ", ".join(p.name for p in sorted(paths, key=lambda p: p.name))
            if on_duplicate == "error":
                dup_errors.append(f"No.{no}: {names}")
                continue
            chosen = max(paths, key=lambda p: p.stat().st_size)
            skipped = [p.name for p in paths if p != chosen]
            print(f"  No.{no}: use {chosen.name} (skip {', '.join(skipped)}, duplicate)")
            out.append((no, chosen))
        else:
            out.append((no, paths[0]))

    if dup_errors:
        msg = "番号が重複しています。1枚だけ残してから再実行してください:\n" + "\n".join(
            f"  - {e}" for e in dup_errors
        )
        raise SystemExit(msg)
    return out


def list_unnumbered_images(original_dir: Path) -> list[Path]:
    return sorted(
        p
        for p in original_dir.iterdir()
        if p.is_file() and p.suffix.lower() in IMAGE_EXTS and not re.fullmatch(r"\d+", p.stem)
    )


def write_tracking_lists(
    original_dir: Path,
    no_to_title: dict[int, str],
    title_to_entry: dict[str, str],
    numbered: list[tuple[int, Path]],
) -> None:
    """Write MISSING_NUMBERS.txt and UNNUMBERED_FILES.txt under original/."""
    have = {no for no, _ in numbered}
    missing_lines = [
        "# 原画未配置（CSV + cards.json に存在するが original/ に番号ファイルなし）",
        "# 更新: python tools_process_card_portraits.py --write-lists",
        "# 形式: No | 日本語名 | 出力ファイル名",
        "#",
        "# No.56-66（Kneel! 等）は No.48 調教コマンドと同一絵のため、別原画は不要",
        "",
    ]
    shared_targets = set()
    for extras in SHARED_EXTRA_ENTRIES.values():
        for entry in extras:
            for no, title in no_to_title.items():
                if title_to_entry.get(title) == entry:
                    shared_targets.add(no)
                    break

    for no in sorted(no_to_title):
        title = no_to_title[no]
        entry = title_to_entry.get(title)
        if entry is None:
            continue
        if no in have:
            continue
        if no in shared_targets:
            continue
        missing_lines.append(f"{no}\t{title}\t{entry}.png")

    unnumbered = list_unnumbered_images(original_dir)
    unnum_lines = [
        "# 番号未指定ファイル（{No}.ext 形式ではない画像）",
        "# カード原画としては使われません。番号付きファイル名にリネームしてください。",
        "",
    ]
    if unnumbered:
        for p in unnumbered:
            unnum_lines.append(p.name)
    else:
        unnum_lines.append("(なし)")

    (original_dir / "MISSING_NUMBERS.txt").write_text(
        "\n".join(missing_lines) + "\n", encoding="utf-8"
    )
    (original_dir / "UNNUMBERED_FILES.txt").write_text(
        "\n".join(unnum_lines) + "\n", encoding="utf-8"
    )
    print(f"Wrote {original_dir / 'MISSING_NUMBERS.txt'} ({len(missing_lines) - 4} entries)")
    print(f"Wrote {original_dir / 'UNNUMBERED_FILES.txt'}")


def main() -> int:
    parser = argparse.ArgumentParser(description="Process numbered card portrait originals")
    parser.add_argument(
        "--csv",
        type=Path,
        default=None,
        help="CSV/JSON with No + Japanese card name (default: auto-detect)",
    )
    parser.add_argument("--original", type=Path, default=ORIGINAL_DIR)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument(
        "--on-duplicate",
        choices=("error", "largest"),
        default="error",
        help="Duplicate CSV numbers in original/ (default: error)",
    )
    parser.add_argument(
        "--write-lists",
        action="store_true",
        help="Write MISSING_NUMBERS.txt / UNNUMBERED_FILES.txt only (no PNG export)",
    )
    parser.add_argument(
        "--only",
        type=str,
        default="",
        help="Comma-separated CSV numbers to process (default: all in original/)",
    )
    args = parser.parse_args()

    csv_path = args.csv or default_csv_path()
    if not csv_path.is_file():
        print(f"CSV/JSON not found: {csv_path}", file=sys.stderr)
        return 1

    if not args.original.is_dir():
        print(f"original dir not found: {args.original}", file=sys.stderr)
        return 1

    no_to_title = load_no_to_title(csv_path)
    title_to_entry = load_title_to_entry()

    try:
        items = discover_numbered(args.original, on_duplicate=args.on_duplicate)
    except SystemExit as exc:
        print(str(exc), file=sys.stderr)
        return 1

    if args.write_lists:
        write_tracking_lists(args.original, no_to_title, title_to_entry, items)
        return 0

    print(f"Using card list: {csv_path.name}")

    only = {int(x) for x in args.only.split(",") if x.strip().isdigit()} if args.only else None

    if not items:
        print("No numbered images in original/")
        write_tracking_lists(args.original, no_to_title, title_to_entry, items)
        return 0

    write_tracking_lists(args.original, no_to_title, title_to_entry, items)

    ok = 0
    errors: list[str] = []
    methods: dict[str, int] = {"smart": 0}
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
            method = process_one(path, entry, extras=extras, dry_run=args.dry_run)
            methods[method] = methods.get(method, 0) + 1
            ok += 1
        except Exception as exc:  # noqa: BLE001
            errors.append(f"No.{no} ({path.name}): {exc}")

    print(f"\nDone: {ok} processed, {len(errors)} errors")
    print(f"Crop methods: smart={methods.get('smart', 0)}")
    for e in errors:
        print(f"  ERROR: {e}", file=sys.stderr)
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
