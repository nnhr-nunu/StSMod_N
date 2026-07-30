# -*- coding: utf-8 -*-
"""カード説明から UG / Upgrade 注釈を除去する。"""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent

# 末尾の UG 注釈ブロック
PATTERNS = [
    re.compile(r"\n\[gold\]UG:.*?\[/gold\]\s*$", re.DOTALL),
    re.compile(r"\n\[gold\]Upgrade:.*?\[/gold\]\s*$", re.DOTALL | re.IGNORECASE),
    re.compile(r"。アップグレードで廃棄が消える。?$"),
    re.compile(r"。アップグレードで全敵に適用。?$"),
    re.compile(r"。アップグレード後：[^。\n]+。?$"),
    re.compile(r"\n\[gold\]Upgrade:.*?\[/gold\]", re.DOTALL | re.IGNORECASE),
]


def strip_desc(text: str) -> str:
    prev = None
    while prev != text:
        prev = text
        for pat in PATTERNS:
            text = pat.sub("", text)
    return text.rstrip()


def process(path: Path) -> int:
    data = json.loads(path.read_text(encoding="utf-8"))
    n = 0
    for k, v in list(data.items()):
        if not k.endswith(".description") or not isinstance(v, str):
            continue
        cleaned = strip_desc(v)
        if cleaned != v:
            data[k] = cleaned
            n += 1
            print(f"{path.name} {k}")
            print(f"  - {v!r}")
            print(f"  + {cleaned!r}")
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return n


def main() -> None:
    total = 0
    for rel in (
        "HypnosisCreator/localization/jpn/cards.json",
        "HypnosisCreator/localization/eng/cards.json",
    ):
        total += process(ROOT / rel)
    print(f"stripped {total} descriptions")


if __name__ == "__main__":
    main()
