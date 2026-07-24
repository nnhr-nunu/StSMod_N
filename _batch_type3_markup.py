# -*- coding: utf-8 -*-
"""Type-3 batch: apply StS markup to jpn card descriptions where semantics already match CSV."""
from __future__ import annotations

import csv
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT))
import _audit_cards as audit  # noqa: E402

CSV_PATH = pathlib.Path(r"C:\Users\homut\Downloads\2026-07-25ヒプノクリエイター - シート1.csv")
LOC_PATH = ROOT / "HypnosisCreator" / "localization" / "jpn" / "cards.json"
SKIP_NOS = {
    12, 28, 33, 47, 52, 71, 80, 86, 90, 94, 96, 100, 101, 102, 68, 99,
}

GOLD_JP = [
    "トランス", "ブロック", "破滅", "沼", "カウント", "手札", "廃棄", "アタック",
    "推し活", "催眠系カウント", "エナジー", "筋力", "弱体", "脆弱", "脱力",
    "性癖", "リテイン", "廃棄しない",
]
GOLD_EN = [
    "Trance", "Block", "Doom", "Bog", "Count", "Hand", "Exhaust", "Attack",
    "Oshi Activity", "Energy", "Strength", "Weak", "Vulnerable", "Frail", "Fetish",
]

PREVIEW_PAREN = re.compile(
    r"[（(](?:合計)?N[^）)]*[）)]|"
    r"[（(]現在[^）)]*[）)]|"
    r"[（(].*?ダメージ.*?[）)]",
)


def strip_markup(s: str) -> str:
    s = re.sub(r"\[/?\w+\]", "", s)
    s = re.sub(r"\{[^}]+\}", "N", s)
    return re.sub(r"\s+", "", s)


def normalize_csv(effect: str) -> str:
    s = effect.strip()
    s = PREVIEW_PAREN.sub("", s)
    for tail in ("廃棄。", "廃棄", "保留。", "保留"):
        if s.endswith(tail):
            s = s[: -len(tail)].rstrip("。")
    return strip_markup(s)


def normalize_loc(desc: str) -> str:
    return strip_markup(desc or "")


def csv_to_jpn_markup(effect: str, existing: str | None) -> str:
  """Convert CSV prose to loc markup, preserving existing {Var:diff()} tokens when possible."""
  s = effect.strip()
  s = PREVIEW_PAREN.sub("", s)
  if s.endswith("廃棄。"):
      s = s[:-3] + "。"
  elif s.endswith("廃棄"):
      s = s[:-2]
  if s.endswith("保留。"):
      s = s[:-3] + "。"
  elif s.endswith("保留"):
      s = s[:-2]

  # Reuse dynamic placeholders from existing loc for numbers that appear in both
  if existing:
      for m in re.finditer(r"\{(\w+):diff\(\)\}", existing):
          token = m.group(0)
          name = m.group(1)
          # keep token in output by replacing likely number patterns later — handled below
          _ = name

  # Keywords longest-first
  for kw in sorted(GOLD_JP, key=len, reverse=True):
      if kw in s:
          s = s.replace(kw, f"[gold]{kw}[/gold]")

  # Hypnosis count compound
  s = s.replace("[gold]催眠系[/gold][gold]カウント[/gold]", "[gold]催眠系カウント[/gold]")

  # If existing has placeholders, prefer keeping structure: only add [blue] to bare integers not inside tags
  def blue_int(m: re.Match[str]) -> str:
      return f"[blue]{m.group(1)}[/blue]"

  parts: list[str] = []
  pos = 0
  for m in re.finditer(r"\[gold\]|\[blue\]|\{", s):
      parts.append(s[pos : m.start()])
      pos = m.start()
      break
  # Simple pass: blue-wrap standalone digits not already tagged
  result = re.sub(
      r"(?<!\[blue\])(?<!\[gold\])\b(\d+)\b(?![^\[]*\[/blue\])",
      blue_int,
      s,
  )
  # Restore existing dynamic vars from loc when numeric bases match
  if existing:
      for token in re.findall(r"\{[^}]+\}", existing):
          base = re.match(r"\{(\w+):", token)
          if not base:
              continue
          # leave existing sophisticated lines alone if they already have 2+ placeholders
      if existing.count("{") >= 1 and normalize_loc(existing) == normalize_csv(effect):
          return existing
  return result


def main() -> None:
    audit.CSV_PATH = CSV_PATH
    csv_cards, _ = audit.parse_csv_cards()
    loc = json.loads(LOC_PATH.read_text(encoding="utf-8"))
    code_cards = audit.parse_card_classes()
    audit.resolve_loc_keys(code_cards, loc)
    by_title: dict[str, list] = {}
    for c in code_cards:
        if c.get("loc_title"):
            by_title.setdefault(c["loc_title"], []).append(c)

    updated: list[str] = []
    skipped = 0
    for row in csv_cards:
        if row["No"] in SKIP_NOS:
            skipped += 1
            continue
        match = None
        alias = audit.CSV_NAME_TO_CLASS.get(row["name"])
        by_class = {c["class"]: c for c in code_cards}
        if alias and alias in by_class:
            match = by_class[alias]
        if match is None:
            for c in by_title.get(row["name"], []):
                match = c
                break
        if match is None:
            continue

        key = match["loc_key"] + ".description"
        cur = loc.get(key, "")
        csv_norm = normalize_csv(row["effect"])
        loc_norm = normalize_loc(cur)
        if not csv_norm or not cur:
            continue
        if csv_norm == loc_norm:
            new = csv_to_jpn_markup(row["effect"], cur)
            if new != cur:
                loc[key] = new
                updated.append(f"No.{row['No']:3d} {row['name']} -> {match['class']}")
            continue
        # semantic close: loc contains csv core
        if csv_norm in loc_norm or loc_norm in csv_norm:
            new = csv_to_jpn_markup(row["effect"], cur)
            if new != cur:
                loc[key] = new
                updated.append(f"No.{row['No']:3d} {row['name']} (partial) -> {match['class']}")

    LOC_PATH.write_text(json.dumps(loc, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Updated {len(updated)} descriptions (skipped explicit {skipped})")
    for line in updated[:40]:
        print(" ", line)
    if len(updated) > 40:
        print(f"  ... and {len(updated) - 40} more")


if __name__ == "__main__":
    main()
