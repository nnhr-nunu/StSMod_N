# -*- coding: utf-8 -*-
import json
from pathlib import Path

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")

# key -> (jpn, eng)
UPDATES = {
    "ASSASSIN_RAIDER_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage to a random enemy.",
    ),
    "CROSSBOW_RAIDER_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage to a random enemy.",
    ),
    "SNEAKY_GREMLIN_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage to a random enemy.",
    ),
    "OBSCURA_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage to a random enemy.",
    ),
    "SEAPUNK_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}×{Hits:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage {Hits:diff()} times to a random enemy.",
    ),
    "HUNTER_KILLER_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}×{Hits:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage {Hits:diff()} times to a random enemy.",
    ),
    "ENTMANCER_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}×{Hits:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage {Hits:diff()} times to a random enemy.",
    ),
    "TURRET_OPERATOR_HEART": (
        "希少な心臓。ランダムな相手に{Damage:diff()}×{Hits:diff()}ダメージを与える。",
        "Rare Heart. Deal {Damage:diff()} damage {Hits:diff()} times to a random enemy.",
    ),
    "NIBBIT_HEART": (
        "希少な心臓。{Block:diff()}ブロックを得る。",
        "Rare Heart. Gain {Block:diff()} Block.",
    ),
    "AXE_RAIDER_HEART": (
        "希少な心臓。{Block:diff()}ブロックを得る。",
        "Rare Heart. Gain {Block:diff()} Block.",
    ),
    "BOWL_BUG_EGG_HEART": (
        "希少な心臓。{Block:diff()}ブロックを得る。",
        "Rare Heart. Gain {Block:diff()} Block.",
    ),
    "PROGENITOR_BUG_HEART": (
        "希少な心臓。{Block:diff()}ブロックを得る。",
        "Rare Heart. Gain {Block:diff()} Block.",
    ),
    "TUNNELOR_HEART": (
        "希少な心臓。{Block:diff()}ブロックを得る。",
        "Rare Heart. Gain {Block:diff()} Block.",
    ),
    "OVICOPTER_HEART": (
        "希少な心臓。所有している心臓レリック1つにつき2ブロックを得る（合計 {Block:diff()}）。",
        "Rare Heart. Gain 2 Block for each Heart relic you have (total {Block:diff()}).",
    ),
    "GLOBE_HEAD_HEART": (
        "希少な心臓。パワーカードプレイ時に{Block:diff()}ブロックを得る。",
        "Rare Heart. Whenever you play a Power, gain {Block:diff()} Block.",
    ),
    "STOLEN_HEART": (
        "戦闘開始時、所持している奪った心臓1つにつき2ブロックを得る（合計 {Block:diff()}）。",
        "At the start of combat, gain 2 Block for each Stolen Heart (total {Block:diff()}).",
    ),
}

for lang, idx in (("jpn", 0), ("eng", 1)):
    path = ROOT / f"HypnosisCreator/localization/{lang}/relics.json"
    data = json.loads(path.read_text(encoding="utf-8"))
    for key, pair in UPDATES.items():
        data[f"HYPNOSISCREATOR-{key}.description"] = pair[idx]
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("updated", path)
