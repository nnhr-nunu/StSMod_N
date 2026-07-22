#!/usr/bin/env python3
"""Generate HypnosisCreator power icons from card portraits or custom sources."""

from __future__ import annotations

import hashlib
import shutil
from pathlib import Path

from PIL import Image, ImageEnhance

ROOT = Path(__file__).resolve().parent
PORTRAITS = ROOT / "HypnosisCreator" / "images" / "card_portraits"
POWERS = ROOT / "HypnosisCreator" / "images" / "powers"
SOURCES = POWERS / "_sources"

SMALL_SIZE = 64
BIG_SIZE = 256

# power file stem -> card portrait stem (None = custom source)
CARD_POWER_MAP: dict[str, str | None] = {
    "bog_power": None,
    "cult_leader_power": "cult_leader",
    "oshi_activity_power": "proselytize",
    "acceptance_need_power": "acceptance_need",
    "as_you_wish_power": "as_you_wish",
    "rapport_power": "rapport",
    "soften_power": "soften",
    "mastery_power": "mastery",
    "corrosion_power": "corrosion",
    "word_flood_power": "word_flood",
    "fetish_understanding_power": "fetish_understanding",
    "mass_hypnosis_power": "mass_hypnosis",
    "status_hypnosis_power": "status_hypnosis",
    "ericksonian_power": "ericksonian",
    "encore_power": "encore",
    "fudo_myoo_power": "fudo_myoo",
    "catalepsy_power": "catalepsy",
    "sense_share_power": "sense_share",
    "bottomless_bog_power": "bottomless_bog",
    "tentacle_recall_power": "tentacle_recall",
    "time_stop_mark_power": "time_stop_strike",
    "bare_instinct_power": "bare_instinct",
    "abyss_cheer_return_power": "cheer_from_the_abyss",
    "asmr_hypnosis_power": "asmr_hypnosis",
    "brain_slime_redirect_power": "brain_slime_hypnosis",
    "breath_control_power": "breath_control",
    "cognitive_shuffle_power": "cognitive_shuffle",
    "cognitive_void_bypass_power": "cognitive_shuffle",
    "love_hypnosis_power": "love_hypnosis",
    "plant_parasite_mark_power": "plant_parasite_hypnosis",
    "sensitivity_power": "sensitivity3000",
    "slime_hypnosis_power": "slime_hypnosis",
    "total_control_power": "total_control",
    "unconscious_guidance_power": "unconscious_guidance",
    "zero_out_power": "zero_out",
    "fetish_plant_pending_power": "beginner_hypnosis",
    "next_turn_asleep_power": "poor_sleep",
}

SKIP_POWERS = {
    "power",
    "abnormal_fetish_power",
    "ds_fetish_power",
    "sm_fetish_power",
    "trance_fetish_power",
    "trance_power",
    "cardiac_arrest_power",
}

NU_SOURCE = Path(
    r"C:\Users\homut\.cursor\projects\d-Dev-antigravity-StSMod-N\assets"
    r"\c__Users_homut_AppData_Roaming_Cursor_User_workspaceStorage_empty-window_images_nu-5536722d-b7f0-4850-a1d2-308ca67f3443.png"
)
NU_PROJECT_SOURCE = SOURCES / "bog_nu.png"


def placeholder_hash() -> str:
    return hashlib.md5((POWERS / "power.png").read_bytes()).hexdigest()


def is_placeholder(path: Path) -> bool:
    if not path.exists():
        return True
    return hashlib.md5(path.read_bytes()).hexdigest() == placeholder_hash()


def portrait_path(stem: str, *, big: bool) -> Path | None:
    folder = PORTRAITS / ("big" if big else "")
    path = folder / f"{stem}.png"
    return path if path.exists() else None


def center_square_crop(img: Image.Image) -> Image.Image:
    w, h = img.size
    side = min(w, h)
    left = (w - side) // 2
    top = (h - side) // 2
    return img.crop((left, top, left + side, top + side))


def make_icon(content: Image.Image, size: int) -> Image.Image:
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    pad = max(2, size // 16)
    fit = size - pad * 2

    img = content.copy()
    if img.mode != "RGBA":
        img = img.convert("RGBA")
    img.thumbnail((fit, fit), Image.Resampling.LANCZOS)

    rgb = ImageEnhance.Contrast(img.convert("RGB")).enhance(1.05)
    alpha = img.split()[-1]
    img = Image.merge("RGBA", (*rgb.split(), alpha))

    ox = (size - img.width) // 2
    oy = (size - img.height) // 2
    canvas.alpha_composite(img, (ox, oy))
    return canvas


def load_card_content(stem: str) -> Image.Image | None:
    src = portrait_path(stem, big=True) or portrait_path(stem, big=False)
    if src is None:
        return None
    return center_square_crop(Image.open(src).convert("RGBA"))


def remove_near_black_background(img: Image.Image, threshold: int = 32) -> Image.Image:
    rgba = img.convert("RGBA")
    pixels = rgba.load()
    w, h = rgba.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            if a == 0:
                continue
            if r < threshold and g < threshold and b < threshold:
                pixels[x, y] = (0, 0, 0, 0)
    bbox = rgba.getbbox()
    return rgba.crop(bbox) if bbox else rgba


def load_bog_nu_content() -> Image.Image:
    SOURCES.mkdir(parents=True, exist_ok=True)
    if not NU_PROJECT_SOURCE.exists():
        shutil.copy2(NU_SOURCE, NU_PROJECT_SOURCE)
    return remove_near_black_background(Image.open(NU_PROJECT_SOURCE))


def save_pair(stem: str, content: Image.Image) -> None:
    small = make_icon(content, SMALL_SIZE)
    big = make_icon(content, BIG_SIZE)
    (POWERS / "big").mkdir(parents=True, exist_ok=True)
    small.save(POWERS / f"{stem}.png")
    big.save(POWERS / "big" / f"{stem}.png")


def main() -> int:
    missing_cards: list[str] = []
    generated: list[str] = []
    skipped: list[str] = []

    for power_stem, card_stem in CARD_POWER_MAP.items():
        if power_stem in SKIP_POWERS:
            skipped.append(power_stem)
            continue

        out_small = POWERS / f"{power_stem}.png"
        if out_small.exists() and not is_placeholder(out_small) and power_stem != "bog_power":
            # Keep existing custom icons unless they are still placeholders.
            skipped.append(power_stem)
            continue

        if power_stem == "bog_power":
            content = load_bog_nu_content()
        else:
            assert card_stem is not None
            content = load_card_content(card_stem)
            if content is None:
                missing_cards.append(f"{power_stem} <- {card_stem}.png")
                continue

        save_pair(power_stem, content)
        generated.append(power_stem)

    print("generated:", len(generated))
    for name in generated:
        print("  +", name)
    print("skipped:", len(skipped))
    if missing_cards:
        print("missing card portraits:")
        for line in missing_cards:
            print("  !", line)
    return 0 if not missing_cards else 1


if __name__ == "__main__":
    raise SystemExit(main())
