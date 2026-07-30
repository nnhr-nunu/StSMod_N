# -*- coding: utf-8 -*-
"""Extract No->Class mapping from audit report."""
import re
from pathlib import Path

text = Path("_card_audit_report.txt").read_text(encoding="utf-8")
pat = re.compile(
    r"No\.\s*(\d+)\s*\|\s*CSV:(.*?)\s*\|\s*Class:(\w+)\s*\((\w+)\)\s*\|\s*locTitle:(.*?)\s*\|\s*CardRarity\.(\w+)\s+cost=(\S+)\s+CardType\.(\w+)"
)
for m in pat.finditer(text):
    no, csv_name, cls, folder, loc, rar, cost, typ = m.groups()
    print(f"{int(no):3d}|{cls}|{csv_name.strip()}|{rar}|{cost}|{typ}|loc={loc.strip()}")
