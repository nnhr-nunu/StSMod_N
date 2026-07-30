import re
from pathlib import Path
hearts = Path(r"D:\Dev\antigravity\StSMod_N\HypnosisCreatorCode\Relics\Hearts")
ids = {l.split("\t")[0] for l in Path(r"D:\Dev\antigravity\StSMod_N\_monster_ids_dump.txt").read_text(encoding="utf-8").splitlines() if l.strip() and not l.startswith("FAIL")}
miss = []
count = 0
for p in sorted(hearts.glob("*Heart.cs")):
    t = p.read_text(encoding="utf-8")
    if "class" not in t or "EnemyHeartRelic" not in t:
        continue
    count += 1
    if "MonsterIdEntries" in t:
        block = t.split("MonsterIdEntries", 1)[1].split(";", 1)[0]
        allids = re.findall(r'"([^"]+)"', block)
    else:
        m = re.search(r'MonsterIdEntry\s*=>\s*"([^"]+)"', t)
        allids = [m.group(1)] if m else []
    for i in allids:
        if i not in ids:
            miss.append((p.name, i))
print(f"enemy hearts={count} miss={miss}")
# required CSV ids sample
need = ["LEAF_SLIME_S","CRUSHER","ROCKET","MYTE","BOWLBUG_NECTAR","THE_INSATIABLE","DECIMILLIPEDE_SEGMENT_FRONT"]
for n in need:
    print(n, "in dump", n in ids)
