import json
from pathlib import Path
for path in [Path("_csv_cards_latest.json"), Path("_hearts_csv_111_187.json"), Path("_csv_cards_1_104.json")]:
    if not path.exists():
        continue
    data = json.loads(path.read_text(encoding="utf-8"))
    print("===", path)
    for c in data:
        s = json.dumps(c, ensure_ascii=False)
        if any(k in s for k in ("状態異常催眠", "フクロウ", "バーサーカー", "OWL", "BERSERK", "Magistrate")):
            print(s[:500])
            print("---")
