#!/usr/bin/env python3
"""Generate HypnosisCreator power icons from card portraits or custom sources.

Medal framing applies only to card-granted powers (portrait cutouts).
Non-card art such as bog (ぬ) uses a plain transparent icon.
"""

from __future__ import annotations

import argparse
import hashlib
import shutil
from pathlib import Path

from PIL import Image

from tools_power_icon_style import (
    center_square_crop,
    make_plain_icon,
    make_power_icon,
)

ROOT = Path(__file__).resolve().parent
PORTRAITS = ROOT / "HypnosisCreator" / "images" / "card_portraits"
POWERS = ROOT / "HypnosisCreator" / "images" / "powers"
SOURCES = POWERS / "_sources"

SMALL_SIZE = 64
BIG_SIZE = 256

# Card-granted powers only: power stem -> granting card portrait stem
CARD_DERIVED_POWER_MAP: dict[str, str] = {
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

# Never overwritten by this generator (custom art / relic icons / vanilla-style)
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


def save_card_derived_pair(stem: str, content: Image.Image) -> None:
    small = make_power_icon(content, SMALL_SIZE)
    big = make_power_icon(content, BIG_SIZE)
    (POWERS / "big").mkdir(parents=True, exist_ok=True)
    small.save(POWERS / f"{stem}.png")
    big.save(POWERS / "big" / f"{stem}.png")


def save_bog_pair(content: Image.Image) -> None:
    small = make_plain_icon(content, SMALL_SIZE)
    big = make_plain_icon(content, BIG_SIZE)
    (POWERS / "big").mkdir(parents=True, exist_ok=True)
    small.save(POWERS / "bog_power.png")
    big.save(POWERS / "big" / "bog_power.png")


def should_generate(power_stem: str, *, force: bool) -> bool:
    if power_stem in SKIP_POWERS:
        return False
    if force:
        return True
    return is_placeholder(POWERS / f"{power_stem}.png")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--force",
        action="store_true",
        help="Regenerate card-derived icons even when a non-placeholder file exists.",
    )
    parser.add_argument(
        "--bog-only",
        action="store_true",
        help="Regenerate bog (ぬ) plain icon only.",
    )
    args = parser.parse_args()

    missing_cards: list[str] = []
    generated: list[str] = []
    skipped: list[str] = []

    if args.bog_only:
        save_bog_pair(load_bog_nu_content())
        print("generated: 1")
        print("  + bog_power")
        return 0

    for power_stem, card_stem in CARD_DERIVED_POWER_MAP.items():
        if not should_generate(power_stem, force=args.force):
            skipped.append(power_stem)
            continue

        content = load_card_content(card_stem)
        if content is None:
            missing_cards.append(f"{power_stem} <- {card_stem}.png")
            continue

        save_card_derived_pair(power_stem, content)
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
