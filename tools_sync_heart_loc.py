# -*- coding: utf-8 -*-
"""Sync eng/jpn relics.json heart descriptions from CSV; add new hearts + powers."""
import json
import re
from pathlib import Path

ROOT = Path(r"D:\Dev\antigravity\StSMod_N")
csv_rows = {int(r["No"]): r for r in json.loads((ROOT / "_hearts_csv_111_187.json").read_text(encoding="utf-8"))}

# No -> localization key suffix (without HYPNOSISCREATOR-)
NO_TO_KEY = {
    111: "LEAF_SLIME_SMALL_HEART",
    112: "TWIG_SLIME_SMALL_HEART",
    113: "LEAF_SLIME_MED_HEART",
    114: "TWIG_SLIME_MED_HEART",
    115: "SHRINKER_BEETLE_HEART",
    116: "BRISTLE_WORM_HEART",
    117: "INKLET_HEART",
    118: "CUBEX_HEART",
    119: "NIBBIT_HEART",
    120: "AXE_RAIDER_HEART",
    121: "TRACKER_RAIDER_HEART",
    122: "CROSSBOW_RAIDER_HEART",
    123: "ASSASSIN_RAIDER_HEART",
    124: "BRUTE_RAIDER_HEART",
    125: "JACKFRUIT_HEART",
    126: "FLYCONID_HEART",
    127: "VINE_SHAMBLER_HEART",
    128: "FOG_MOG_HEART",
    129: "CONSTRICTOR_HEART",
    130: "MAULER_HEART",
    131: "BYRDNIS_HEART",
    132: "ANCIENT_IDOL_HEART",
    133: "WRIGGLER_HEART",
    134: "RITUAL_BEAST_HEART",
    135: "BLOOD_PRIEST_HEART",
    136: "PHANTOM_HEART",
    137: "SEAPUNK_HEART",
    138: "TOAD_KING_HEART",
    139: "CORPSE_SLUG_HEART",
    140: "SLUDGE_SPINNER_HEART",
    141: "SNEAKY_GREMLIN_HEART",
    142: "FAT_GREMLIN_HEART",
    143: "SEWER_SHELL_HEART",
    144: "LIVING_FOG_HEART",
    145: "PUNCH_MACHINE_HEART",
    146: "ROCK_CULTIST_HEART",
    147: "WET_CULTIST_HEART",
    148: "GHOST_SHIP_HEART",
    149: "FOSSIL_STALKER_HEART",
    150: "TWIN_TAIL_RAT_HEART",
    151: "TERROR_EEL_HEART",
    152: "SWARMING_HIVE_HEART",
    153: "PHANTASMAL_BUG_HEART",
    154: "SOUL_FISH_HEART",
    155: "LAGAVULIN_MATRIARCH_HEART",
    156: "WATERFALL_GIANT_HEART",
    157: "EXOSKELETON_BUG_HEART",
    158: "THIEF_GRASSHOPPER_HEART",
    159: "PROGENITOR_BUG_HEART",
    160: "CHOMPER_HEART",
    161: "BOWL_BUG_ROCK_HEART",
    162: "BOWL_BUG_HONEY_HEART",
    163: "BOWL_BUG_SILK_HEART",
    164: "BOWL_BUG_EGG_HEART",
    165: "SPIKE_TOAD_HEART",
    166: "TUNNELOR_HEART",
    167: "MYTE_HEART",
    168: "OBSCURA_HEART",
    169: "HUNTER_KILLER_HEART",
    170: "OVICOPTER_HEART",
    171: "SLEEPING_BEETLE_HEART",
    172: "CENTIPEDE_HEART",
    173: "ENTMANCER_HEART",
    174: "PARASITIZED_PRISM_HEART",
    175: "MAW_BEAST_HEART",
    # 176 Kaiser -> Crusher + Rocket (both)
    177: "KNOWLEDGE_DEMON_HEART",
    178: "LIVING_SHIELD_HEART",
    179: "TURRET_OPERATOR_HEART",
    180: "THE_LOST_HEART",
    181: "THE_FORGOTTEN_HEART",
    182: "AXEBOT_HEART",
    183: "DEVOTED_SCULPTOR_HEART",
    184: "GLOBE_HEAD_HEART",
    185: "SCROLL_OF_BITING_HEART",
    186: "FROG_KNIGHT_HEART",
    187: "FABRICATOR_HEART",
}

ENG_TITLE = {
    "LEAF_SLIME_SMALL_HEART": "Leaf Slime (S) Heart",
    "TWIG_SLIME_SMALL_HEART": "Twig Slime (S) Heart",
    "LEAF_SLIME_MED_HEART": "Leaf Slime (M) Heart",
    "TWIG_SLIME_MED_HEART": "Twig Slime (M) Heart",
    "SHRINKER_BEETLE_HEART": "Shrinker Beetle Heart",
    "BRISTLE_WORM_HEART": "Bristle Worm Heart",
    "INKLET_HEART": "Inklet Heart",
    "CUBEX_HEART": "Cubex Heart",
    "NIBBIT_HEART": "Nibbit Heart",
    "AXE_RAIDER_HEART": "Axe Raider Heart",
    "TRACKER_RAIDER_HEART": "Tracker Raider Heart",
    "CROSSBOW_RAIDER_HEART": "Crossbow Raider Heart",
    "ASSASSIN_RAIDER_HEART": "Assassin Raider Heart",
    "BRUTE_RAIDER_HEART": "Brute Raider Heart",
    "JACKFRUIT_HEART": "Snapping Jackfruit Heart",
    "FLYCONID_HEART": "Flyconid Heart",
    "VINE_SHAMBLER_HEART": "Vine Shambler Heart",
    "FOG_MOG_HEART": "Fogmog Heart",
    "CONSTRICTOR_HEART": "Constrictor Heart",
    "MAULER_HEART": "Mawler Heart",
    "BYRDNIS_HEART": "Byrdonis Heart",
    "ANCIENT_IDOL_HEART": "Bygone Effigy Heart",
    "WRIGGLER_HEART": "Wriggler Heart",
    "RITUAL_BEAST_HEART": "Ceremonial Beast Heart",
    "BLOOD_PRIEST_HEART": "Kin Priest Heart",
    "PHANTOM_HEART": "Vantom Heart",
    "SEAPUNK_HEART": "Seapunk Heart",
    "TOAD_KING_HEART": "Toadpole Heart",
    "CORPSE_SLUG_HEART": "Corpse Slug Heart",
    "SLUDGE_SPINNER_HEART": "Sludge Spinner Heart",
    "SNEAKY_GREMLIN_HEART": "Sneaky Gremlin Heart",
    "FAT_GREMLIN_HEART": "Fat Gremlin Heart",
    "SEWER_SHELL_HEART": "Sewer Clam Heart",
    "LIVING_FOG_HEART": "Living Fog Heart",
    "PUNCH_MACHINE_HEART": "Punch Construct Heart",
    "ROCK_CULTIST_HEART": "Calcified Cultist Heart",
    "WET_CULTIST_HEART": "Damp Cultist Heart",
    "GHOST_SHIP_HEART": "Haunted Ship Heart",
    "FOSSIL_STALKER_HEART": "Fossil Stalker Heart",
    "TWIN_TAIL_RAT_HEART": "Two-Tailed Rat Heart",
    "TERROR_EEL_HEART": "Terror Eel Heart",
    "SWARMING_HIVE_HEART": "Skulking Colony Heart",
    "PHANTASMAL_BUG_HEART": "Phantasmal Gardener Heart",
    "SOUL_FISH_HEART": "Soul Fysh Heart",
    "LAGAVULIN_MATRIARCH_HEART": "Lagavulin Matriarch Heart",
    "WATERFALL_GIANT_HEART": "Waterfall Giant Heart",
    "EXOSKELETON_BUG_HEART": "Exoskeleton Heart",
    "THIEF_GRASSHOPPER_HEART": "Thieving Hopper Heart",
    "PROGENITOR_BUG_HEART": "Louse Progenitor Heart",
    "CHOMPER_HEART": "Chomper Heart",
    "BOWL_BUG_ROCK_HEART": "Bowlbug (Rock) Heart",
    "BOWL_BUG_HONEY_HEART": "Bowlbug (Nectar) Heart",
    "BOWL_BUG_SILK_HEART": "Bowlbug (Silk) Heart",
    "BOWL_BUG_EGG_HEART": "Bowlbug (Egg) Heart",
    "SPIKE_TOAD_HEART": "Spiny Toad Heart",
    "TUNNELOR_HEART": "Tunneler Heart",
    "MYTE_HEART": "Myte Heart",
    "OBSCURA_HEART": "The Obscura Heart",
    "HUNTER_KILLER_HEART": "Hunter Killer Heart",
    "OVICOPTER_HEART": "Ovicopter Heart",
    "SLEEPING_BEETLE_HEART": "Slumbering Beetle Heart",
    "CENTIPEDE_HEART": "Decimillipede Heart",
    "ENTMANCER_HEART": "Entomancer Heart",
    "PARASITIZED_PRISM_HEART": "Infested Prism Heart",
    "MAW_BEAST_HEART": "The Insatiable Heart",
    "CRUSHER_HEART": "Crusher Heart",
    "ROCKET_HEART": "Rocket Heart",
    "KNOWLEDGE_DEMON_HEART": "Knowledge Demon Heart",
    "LIVING_SHIELD_HEART": "Living Shield Heart",
    "TURRET_OPERATOR_HEART": "Turret Operator Heart",
    "THE_LOST_HEART": "The Lost Heart",
    "THE_FORGOTTEN_HEART": "The Forgotten Heart",
    "AXEBOT_HEART": "Axebot Heart",
    "DEVOTED_SCULPTOR_HEART": "Devoted Sculptor Heart",
    "GLOBE_HEAD_HEART": "Globe Head Heart",
    "SCROLL_OF_BITING_HEART": "Scroll of Biting Heart",
    "FROG_KNIGHT_HEART": "Frog Knight Heart",
    "FABRICATOR_HEART": "Fabricator Heart",
}

ENG_DESC = {
    # Only overrides where JP→EN needs care; else simple prefix
}

def jpn_title(csv_name: str) -> str:
    name = csv_name.strip()
    if name.endswith("の心臓"):
        return name
    # リーフスライム(小)の心臓 etc already have の心臓 in early rows
    if "心臓" in name:
        return name
    return f"{name}の心臓"


def ensure_rare_prefix(desc: str) -> str:
    d = (desc or "").strip()
    if not d:
        return "希少な心臓。"
    if not d.startswith("希少な心臓"):
        return "希少な心臓。" + d
    return d


def eng_desc_from_jpn(jpn_desc: str, key: str) -> str:
    # Keep existing-style English; for new/changed use literal translation helpers
    mapping = {
        "希少な心臓。手札に0コストの粘液を1枚加える。": "Rare Heart. Add a 0-cost Slimed to your hand.",
        "希少な心臓。最大HP2を得る。": "Rare Heart. Gain 2 Max HP.",
        "希少な心臓。甲虫ジュースを得る。": "Rare Heart. Obtain Beetle Juice.",
        "希少な心臓。筋力1を得る。": "Rare Heart. Gain 1 Strength.",
        "希少な心臓。スリップ1を得る。": "Rare Heart. Gain 1 Slippery.",
        "希少な心臓。アーティファクト1を得る。": "Rare Heart. Gain 1 Artifact.",
        "希少な心臓。5ブロックを得る。": "Rare Heart. Gain 5 Block.",
        "希少な心臓。ランダムな相手に脆弱2を付与する。": "Rare Heart. Apply 2 Vulnerable to a random enemy.",
        "希少な心臓。ランダムな相手に14ダメージを与える。": "Rare Heart. Deal 14 damage to a random enemy.",
        "希少な心臓。ランダムな相手に10ダメージを与える。": "Rare Heart. Deal 10 damage to a random enemy.",
        "希少な心臓。フルーツジュースを得る。": "Rare Heart. Obtain Fruit Juice.",
        "希少な心臓。ランダムな相手に脆弱1と弱体1を付与する。": "Rare Heart. Apply 1 Vulnerable and 1 Weak to a random enemy.",
        "希少な心臓。25ゴールドを得る。": "Rare Heart. Gain 25 Gold.",
        "希少な心臓。最大HP4を得る。": "Rare Heart. Gain 4 Max HP.",
        "希少な心臓。ランダムな相手に締め付け3を付与する。": "Rare Heart. Apply 3 Constrict to a random enemy.",
        "希少な心臓。ランダムな相手に弱体2を付与する。": "Rare Heart. Apply 2 Weak to a random enemy.",
        "希少な心臓。縄張り意識を得る。": "Rare Heart. Gain Territorial.",
        "希少な心臓。筋力3を得る。": "Rare Heart. Gain 3 Strength.",
        "希少な心臓。手札のランダムなアタック1枚に寄生をエンチャントする。": "Rare Heart. Enchant a random Attack in your hand with Parasite (+3 damage).",
        "希少な心臓。最大HP10を得る。": "Rare Heart. Gain 10 Max HP.",
        "希少な心臓。最大HP5を得る。ゴールドを50得る。": "Rare Heart. Gain 5 Max HP and 50 Gold.",
        "希少な心臓。スリップ2を得る。": "Rare Heart. Gain 2 Slippery.",
        "希少な心臓。ランダムな相手に2×4ダメージを与える。": "Rare Heart. Deal 2 damage 4 times to a random enemy.",
        "希少な心臓。トゲ2を得る。": "Rare Heart. Gain 2 Thorns.",
        "希少な心臓。脱力1を付与する。": "Rare Heart. Apply 1 Frail to a random enemy.",
        "希少な心臓。ランダムな相手に9ダメージを与える。": "Rare Heart. Deal 9 damage to a random enemy.",
        "希少な心臓。プレート4を得る。": "Rare Heart. Gain 4 Plated Armor.",
        "希少な心臓。爆弾ポーションを獲得する。": "Rare Heart. Obtain an Explosive Ampoule.",
        "希少な心臓。儀式1を得る。": "Rare Heart. Gain 1 Ritual.",
        "希少な心臓。55ゴールドを得る。": "Rare Heart. Gain 55 Gold.",
        "希少な心臓。吸血1を得る。": "Rare Heart. Gain 1 Devour Life.",
        "希少な心臓。活力6を得る。": "Rare Heart. Gain 6 Vigor.",
        "希少な心臓。この戦闘中、1ターンに20以上のHPを失わない。": "Rare Heart. This combat, do not lose more than 20 HP in a single turn.",
        "希少な心臓。臆病6を得る。": "Rare Heart. Gain 6 Skittish.",
        "希少な心臓。霊体1を得る。": "Rare Heart. Gain 1 Wraith Form.",
        "希少な心臓。プレート6を得る。": "Rare Heart. Gain 6 Plated Armor.",
        "希少な心臓。100ゴールドを得る。": "Rare Heart. Gain 100 Gold.",
        "希少な心臓。このターン、不死身9を得る。": "Rare Heart. Gain 9 Buffer this turn.",
        "希少な心臓。50ゴールドを得る。": "Rare Heart. Gain 50 Gold.",
        "希少な心臓。14ブロックを得る。": "Rare Heart. Gain 14 Block.",
        "希少な心臓。アーティファクト2を得る。": "Rare Heart. Gain 2 Artifact.",
        "希少な心臓。岩っぽいポーションを得る。": "Rare Heart. Obtain a Potion-Shaped Rock.",
        "希少な心臓。筋力2を得る。": "Rare Heart. Gain 2 Strength.",
        "希少な心臓。ランダムな相手に脆弱1を付与する。": "Rare Heart. Apply 1 Vulnerable to a random enemy.",
        "希少な心臓。7ブロックを得る。": "Rare Heart. Gain 7 Block.",
        "希少な心臓。トゲ5を得る。": "Rare Heart. Gain 5 Thorns.",
        "希少な心臓。32ブロックを得る。": "Rare Heart. Gain 32 Block.",
        "希少な心臓。ランダムな相手に毒5を2回付与する。": "Rare Heart. Apply 5 Poison twice to a random enemy.",
        "希少な心臓。ランダムな相手に16ダメージを与える。": "Rare Heart. Deal 16 damage to a random enemy.",
        "希少な心臓。ランダムな相手に7×3ダメージを与える。": "Rare Heart. Deal 7 damage 3 times to a random enemy.",
        "希少な心臓。所有している心臓レリック1つにつき2ブロックを得る。": "Rare Heart. Gain 2 Block for each Heart relic you have.",
        "希少な心臓。死亡時、HP25で1度だけ復活するバフを得る。": "Rare Heart. Gain Decimillipede. The next time you would die, heal 25 HP instead.",
        "希少な心臓。ランダムな相手に3×7ダメージを与える。": "Rare Heart. Deal 3 damage 7 times to a random enemy.",
        "希少な心臓。スキルポーションを得る。": "Rare Heart. Obtain a Skill Potion.",
        "希少な心臓。ぬぬ地獄を得る。引き寄せ効果のあるカードが追加で15ダメージを与えるようになる。": "Rare Heart. Gain Nunu Hell. Pull cards deal 15 additional damage.",
        "希少な心臓。HPを15回復する。": "Rare Heart. Heal 15 HP.",
        "希少な心臓。プレートアーマー4を得る。": "Rare Heart. Gain 4 Plated Armor.",
        "希少な心臓。ランダムな相手に2×5ダメージを与える。": "Rare Heart. Deal 2 damage 5 times to a random enemy.",
        "希少な心臓。ランダムな相手は筋力2を失う。筋力2を得る。": "Rare Heart. A random enemy loses 2 Strength. Gain 2 Strength.",
        "希少な心臓。ランダムな相手は敏捷2を失う。敏捷2を得る。": "Rare Heart. A random enemy loses 2 Dexterity. Gain 2 Dexterity.",
        "希少な心臓。HPが0になりそうな時、HP1で踏みとどまるバフを得る。残り回数：N回": "Rare Heart. Gain Axebot (start at 2). When you would die, survive at 1 HP and lose 1 charge.",
        "希少な心臓。儀式2を得る。": "Rare Heart. Gain 2 Ritual.",
        "希少な心臓。パワーカードプレイ時に6ブロックを得る。": "Rare Heart. Gain Globe Head. Whenever you play a Power, gain 6 Block.",
        "希少な心臓。ブロックされずにダメージを与えるたび、追加で2ダメージを与えるバフを得る。": "Rare Heart. Gain Scroll of Biting. Whenever you deal unblocked damage, deal 2 additional damage.",
        "希少な心臓。プレートアーマー7を得る。": "Rare Heart. Gain 7 Plated Armor.",
        "希少な心臓。": "Rare Heart.",
    }
    return mapping.get(jpn_desc, "Rare Heart. " + jpn_desc.replace("希少な心臓。", "").strip())


FLAVOR_J = "まだ脈打っている。"
FLAVOR_E = "Still beating."

for lang in ("jpn", "eng"):
    path = ROOT / f"HypnosisCreator/localization/{lang}/relics.json"
    data = json.loads(path.read_text(encoding="utf-8"))

    for no, key in NO_TO_KEY.items():
        row = csv_rows[no]
        jpn_desc = ensure_rare_prefix(row.get("効果説明") or "")
        jtitle = jpn_title(row["カード名称（日本語）"])
        prefix = f"HYPNOSISCREATOR-{key}"
        if lang == "jpn":
            data[f"{prefix}.title"] = jtitle
            data[f"{prefix}.description"] = jpn_desc
            data.setdefault(f"{prefix}.flavor", FLAVOR_J)
        else:
            data[f"{prefix}.title"] = ENG_TITLE.get(key, key.replace("_", " ").title())
            data[f"{prefix}.description"] = eng_desc_from_jpn(jpn_desc, key)
            data.setdefault(f"{prefix}.flavor", FLAVOR_E)

    # Kaiser left/right (CSV 176)
    kaiser_desc_j = "希少な心臓。最大HP10を得る。"
    kaiser_desc_e = "Rare Heart. Gain 10 Max HP."
    for key, jtitle, etitle in (
        ("CRUSHER_HEART", "クラッシャーの心臓", "Crusher Heart"),
        ("ROCKET_HEART", "ロケットの心臓", "Rocket Heart"),
    ):
        prefix = f"HYPNOSISCREATOR-{key}"
        if lang == "jpn":
            data[f"{prefix}.title"] = jtitle
            data[f"{prefix}.description"] = kaiser_desc_j
            data.setdefault(f"{prefix}.flavor", FLAVOR_J)
        else:
            data[f"{prefix}.title"] = etitle
            data[f"{prefix}.description"] = kaiser_desc_e
            data.setdefault(f"{prefix}.flavor", FLAVOR_E)

    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("updated", path)

# Powers loc
POWERS = {
    "CENTIPEDE_REVIVE_POWER": (
        "万足ムカデ",
        "死亡時、HPを25回復して復活する。残り回数: {Amount:diff()}",
        "Decimillipede",
        "The next time you would die, heal 25 HP instead. Charges: {Amount:diff()}",
    ),
    "AXEBOT_SURVIVE_POWER": (
        "アックスマシン",
        "HPが0になりそうな時、HP1で踏みとどまる。残り回数: {Amount:diff()}",
        "Axebot",
        "When you would die, survive at 1 HP. Charges: {Amount:diff()}",
    ),
    "GLOBE_HEAD_POWER": (
        "グローブヘッド",
        "パワーカードをプレイするたび、{Amount:diff()}ブロックを得る。",
        "Globe Head",
        "Whenever you play a Power, gain {Amount:diff()} Block.",
    ),
    "SCROLL_OF_BITING_POWER": (
        "噛みつきの巻物",
        "ブロックされずにダメージを与えるたび、追加で{Amount:diff()}ダメージを与える。",
        "Scroll of Biting",
        "Whenever you deal unblocked damage, deal {Amount:diff()} additional damage.",
    ),
}

for lang in ("jpn", "eng"):
    path = ROOT / f"HypnosisCreator/localization/{lang}/powers.json"
    data = json.loads(path.read_text(encoding="utf-8"))
    for key, (jt, jd, et, ed) in POWERS.items():
        prefix = f"HYPNOSISCREATOR-{key}"
        if lang == "jpn":
            data[f"{prefix}.title"] = jt
            data[f"{prefix}.description"] = jd
        else:
            data[f"{prefix}.title"] = et
            data[f"{prefix}.description"] = ed
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("updated", path)

print("loc sync done")
