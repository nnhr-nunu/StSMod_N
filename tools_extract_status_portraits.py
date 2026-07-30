# -*- coding: utf-8 -*-
"""Extract vanilla status/curse portraits into HypnosisCreator/images/card_portraits."""
from __future__ import annotations

import importlib.util
import re
from pathlib import Path

spec = importlib.util.spec_from_file_location("g", "tools_gen_heart_icons.py")
g = importlib.util.module_from_spec(spec)
spec.loader.exec_module(g)

OUT = Path("HypnosisCreator/images/card_portraits")
PAIRS = [
    ("soot", "images/packed/card_portraits/status/soot.png.import"),
    ("dazed", "images/packed/card_portraits/status/dazed.png.import"),
    ("void_status", "images/packed/card_portraits/status/void.png.import"),
    ("debris", "images/packed/card_portraits/status/debris.png.import"),
    ("frantic_escape", "images/packed/card_portraits/status/frantic_escape.png.import"),
    ("toxic", "images/packed/card_portraits/status/toxic.png.import"),
    ("beckon", "images/packed/card_portraits/status/beckon.png.import"),
    ("infect", "images/packed/card_portraits/status/infection.png.import"),
    ("ascenders_bane", "images/packed/card_portraits/curse/ascenders_bane.png.import"),
    ("injury", "images/packed/card_portraits/curse/injury.png.import"),
    ("greed", "images/packed/card_portraits/curse/greed.png.import"),
    ("doubt", "images/packed/card_portraits/curse/doubt.png.import"),
    ("writhe", "images/packed/card_portraits/curse/writhe.png.import"),
    ("folly", "images/packed/card_portraits/curse/folly.png.import"),
    ("regret", "images/packed/card_portraits/curse/regret.png.import"),
    ("guilty", "images/packed/card_portraits/curse/guilty.png.import"),
    ("curse_of_the_bell", "images/packed/card_portraits/curse/curse_of_the_bell.png.import"),
    ("poor_sleep", "images/packed/card_portraits/curse/poor_sleep.png.import"),
    ("bad_luck", "images/packed/card_portraits/curse/bad_luck.png.import"),
    ("clumsy", "images/packed/card_portraits/curse/clumsy.png.import"),
    ("decay", "images/packed/card_portraits/curse/decay.png.import"),
    ("debt", "images/packed/card_portraits/curse/debt.png.import"),
    ("normality", "images/packed/card_portraits/curse/normality.png.import"),
    ("shame", "images/packed/card_portraits/curse/shame.png.import"),
    ("spore_mind", "images/packed/card_portraits/curse/spore_mind.png.import"),
    ("enthralled", "images/packed/card_portraits/curse/enthralled.png.import"),
    ("abnormal_slime", "images/packed/card_portraits/status/slimed.png.import"),
    ("abnormal_burn", "images/packed/card_portraits/status/burn.png.import"),
    ("abnormal_wound", "images/packed/card_portraits/status/wound.png.import"),
    ("abnormal_wither", "images/packed/card_portraits/status/wither1.png.import"),
]


def resolve_ctex(import_text: str, files: dict) -> str | None:
    m = re.search(r'path="res://(\.godot/imported/[^"]+\.ctex)"', import_text)
    if not m:
        m = re.search(r'path\.s3tc="res://(\.godot/imported/[^"]+\.ctex)"', import_text)
    if not m:
        return None
    ctex = m.group(1)
    if ctex in files:
        return ctex
    stem = Path(ctex).name.split(".png")[0]
    cands = [n for n in files if n.endswith(".ctex") and stem in n]
    if not cands:
        return None
    cands.sort(key=lambda n: ((".s3tc" in n) or (".bptc" in n), len(n)))
    return cands[0]


def main() -> None:
    data, files = g.read_pck_index(g.PCK)
    ok = 0
    for stem, imp in PAIRS:
        if imp not in files:
            print("MISS import", imp)
            continue
        o, s = files[imp]
        text = data[o : o + s].decode("utf-8", "replace")
        ctex = resolve_ctex(text, files)
        if not ctex:
            print("MISS ctex", stem)
            continue
        blob = data[files[ctex][0] : files[ctex][0] + files[ctex][1]]
        try:
            img = g.gst2_to_image(blob)
        except Exception as e:
            print("FAIL", stem, ctex, e)
            continue
        for path in (OUT / f"{stem}.png", OUT / "big" / f"{stem}.png"):
            path.parent.mkdir(parents=True, exist_ok=True)
            img.save(path)
        print("OK", stem, img.size)
        ok += 1
    print(f"done {ok}/{len(PAIRS)}")


if __name__ == "__main__":
    main()
