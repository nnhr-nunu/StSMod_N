# -*- coding: utf-8 -*-
from pathlib import Path
import re

text = Path(r"D:\Dev\antigravity\StSMod_N\_card_audit_report.txt").read_text(encoding="utf-8")
rows = []
for m in re.finditer(
    r"No\.\s*(\d+)\s*\|\s*CSV:([^|]+)\|\s*Class:([^\s(]+)\s*\(([^)]+)\)\s*\|\s*locTitle:([^|]+)\|\s*CardRarity\.(\w+)\s*cost=(\S+)\s*CardType\.(\w+)\n\s*CSV rarity=(\S+)\s+cost=(\S+)\s+type=(\S+)[^\n]*\n\s*ISSUES:\s*(.+)",
    text,
):
    (
        no,
        csvn,
        cls,
        folder,
        loct,
        rar,
        cost,
        typ,
        csv_r,
        csv_c,
        csv_t,
        iss,
    ) = m.groups()
    rows.append(
        {
            "no": int(no),
            "csv": csvn.strip(),
            "cls": cls,
            "folder": folder,
            "loc": loct.strip(),
            "code_r": rar,
            "code_c": cost,
            "code_t": typ,
            "csv_r": csv_r,
            "csv_c": csv_c,
            "csv_t": csv_t,
            "issues": iss.strip(),
        }
    )

print(f"parsed {len(rows)} mappings")
ok = [r for r in rows if r["issues"] == "OK"]
print("OK:", ok)

def has(r, prefix):
    return any(p.strip().startswith(prefix) for p in r["issues"].split(";"))

only_desc = [
    r
    for r in rows
    if has(r, "DESC_MISMATCH")
    and not has(r, "RARITY:")
    and not has(r, "COST:")
    and not has(r, "NAME:")
    and not has(r, "TYPE:")
]
print(f"only DESC (rarity/cost/name/type OK): {len(only_desc)}")
for r in only_desc:
    print(f"  No.{r['no']:3d} {r['cls']:28s} {r['csv']}")

non_token_rarity = [r for r in rows if has(r, "RARITY:") and r["folder"] != "Token"]
token_rarity = [r for r in rows if has(r, "RARITY:") and r["folder"] == "Token"]
print(f"rarity mismatches non-token: {len(non_token_rarity)}")
print(f"rarity mismatches token(CSVコモン vs Token): {len(token_rarity)}")

# Compact mapping with issue flags
print("\nCOMPACT FLAGS (R=rarity C=cost N=name T=type D=desc)")
for r in rows:
    flags = ""
    flags += "R" if has(r, "RARITY:") else "-"
    flags += "C" if has(r, "COST:") else "-"
    flags += "N" if has(r, "NAME:") else "-"
    flags += "T" if has(r, "TYPE:") else "-"
    flags += "D" if has(r, "DESC_MISMATCH") else "-"
    if flags == "-----":
        flags = "OK"
    print(
        f"No.{r['no']:3d} {flags:5s} {r['cls']:28s} csv[{r['csv_r']}/{r['csv_c']}/{r['csv_t']}] "
        f"code[{r['code_r']}/{r['code_c']}/{r['code_t']}] loc={r['loc']!r}"
    )
