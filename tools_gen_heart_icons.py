# -*- coding: utf-8 -*-
"""Generate heart relic icons from StS2 monster Spine atlas textures (GST2/WebP in PCK).

Uses spatlas region bounds to crop head/body pieces (not the whole packed sheet),
so each heart icon is recognizable as that enemy.
"""
from __future__ import annotations

import io
import json
import re
import struct
import subprocess
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFilter, ImageOps
except ImportError:
    import sys

    subprocess.check_call([sys.executable, "-m", "pip", "install", "pillow", "-q"])
    from PIL import Image, ImageDraw, ImageFilter, ImageOps

PCK = Path(r"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck")
HEARTS_DIR = Path(r"D:\Dev\antigravity\StSMod_N\HypnosisCreatorCode\Relics\Hearts")
OUT_RELICS = Path(r"D:\Dev\antigravity\StSMod_N\HypnosisCreator\images\relics")
OUT_BIG = OUT_RELICS / "big"
REPORT = Path(r"D:\Dev\antigravity\StSMod_N\_heart_icon_report.txt")
DUMP_PROJ = Path(r"D:\Dev\antigravity\StSMod_N\tools_reflect\DumpMonsterVisuals")

ICON = 128
BIG = 256

# Shared atlas skins: MonsterIdEntry → region name prefix (e.g. coral/, slug/)
STEM_SKIN_PREFIX: dict[str, str] = {
    "CALCIFIED_CULTIST": "coral/",
    "DAMP_CULTIST": "slug/",
    "BOWLBUG_EGG": "cocoon/",
    "BOWLBUG_NECTAR": "goop/",
    "BOWLBUG_ROCK": "rock/",
    "BOWLBUG_SILK": "web/",
    "TRACKER_RUBY_RAIDER": "tracker/",
    "CRUSHER": "",
    "ROCKET": "",
}

# Heart MonsterIdEntry → preferred atlas stem when auto-match fails
STEM_ALIASES: dict[str, str] = {
    "ASSASSIN_RUBY_RAIDER": "assassin_ruby_raider",
    "AXE_RUBY_RAIDER": "axe_ruby_raider",
    "BRUTE_RUBY_RAIDER": "brute_ruby_raider",
    "CROSSBOW_RUBY_RAIDER": "crossbow_ruby_raider",
    "TRACKER_RUBY_RAIDER": "tracker_ruby_raider",
    "LEAF_SLIME_S": "leaf_slime_s",
    "LEAF_SLIME_M": "leaf_slime_m",
    "TWIG_SLIME_S": "twig_slime_s",
    "TWIG_SLIME_M": "twig_slime_m",
    "DAMP_CULTIST": "cultists",
    "CALCIFIED_CULTIST": "cultists",
    "BYRDONIS": "byrdonis",
    "FOGMOG": "fogmog",
    "HAUNTED_SHIP": "haunted_ship",
    "SOUL_FYSH": "soul_fysh",
    "ENTOMANCER": "entomancer",
    "DECIMILLIPEDE_SEGMENT_FRONT": "decimillipede",
    "DECIMILLIPEDE_SEGMENT_MIDDLE": "decimillipede",
    "DECIMILLIPEDE_SEGMENT_BACK": "decimillipede",
    "BOWLBUG_EGG": "bowlbug",
    "BOWLBUG_NECTAR": "bowlbug",
    "BOWLBUG_ROCK": "bowlbug",
    "BOWLBUG_SILK": "bowlbug",
    "EXOSKELETON": "exoskeleton",
    "PHANTASMAL_GARDENER": "phantasmal_gardener",
    "LOUSE_PROGENITOR": "louse_progenitor",
    "INFESTED_PRISM": "infested_prism",
    "CUBEX_CONSTRUCT": "cubex_construct",
    "CEREMONIAL_BEAST": "ceremonial_beast",
    "KIN_PRIEST": "kin_priest",
    "THIEVING_HOPPER": "thieving_hopper",
    "SHRINKER_BEETLE": "shrinker_beetle",
    "SLUMBERING_BEETLE": "slumbering_beetle",
    "SLUDGE_SPINNER": "sludge_spinner",
    "FUZZY_WURM_CRAWLER": "fuzzy_wurm_crawler",
    "WRIGGLER": "wriggler",
    "VINE_SHAMBLER": "vine_shambler",
    "LIVING_FOG": "living_smog",
    "TWO_TAILED_RAT": "two_tailed_rat",
    "SEWER_CLAM": "sewer_clam",
    "OVICOPTER": "ovicopter",
    "FLYCONID": "flying_mushrooms",
    "MAWLER": "mawler",
    "PUNCH_CONSTRUCT": "punch_construct",
    "TUNNELER": "tunneler",
    "SLITHERING_STRANGLER": "slithering_strangler",
    "SPINY_TOAD": "spiny_toad",
    "TOADPOLE": "toadpole",
    "TERROR_EEL": "terror_eel",
    "SEAPUNK": "seapunk",
    "THE_OBSCURA": "the_obscura",
    "VANTOM": "vantom",
    "THE_INSATIABLE": "the_insatiable",
    "SKULKING_COLONY": "skulking_colony",
    "BYGONE_EFFIGY": "bygone_effigy",
    "SNAPPING_JAXFRUIT": "snapping_jaxfruit",
    "HUNTER_KILLER": "hunter_killer",
    "CRUSHER": "kaiser_crab",
    "ROCKET": "kaiser_crab",
    "MYTE": "myte",
    "KNOWLEDGE_DEMON": "knowledge_demon",
    "OWL_MAGISTRATE": "owl_magistrate",
    "SLIMED_BERSERKER": "slimed_berserker",
    "MECHA_KNIGHT": "mecha_knight",
    "SOUL_NEXUS": "soul_nexus",
    "FLAIL_KNIGHT": "flail_knight",
    "SPECTRAL_KNIGHT": "spectral_knight",
    "MAGI_KNIGHT": "magi_knight",
    "QUEEN": "queen",
    "TEST_SUBJECT": "test_subject",
    "AEONGLASS": "aeonglass",
    # 謎の騎士は見た目がフレイルナイトと同一
    "MYSTERIOUS_KNIGHT": "flail_knight",
    # 偽商人は通常商人（shop_merchant_top）の見た目を使う
    "FAKE_MERCHANT_MONSTER": "shop_merchant_top",
    # バトルフレンド心臓は V2 スキン（battleworn_dummy）
    "BATTLE_FRIEND_V1": "battleworn_dummy",
    "CHOMPER": "chomper",
    "CORPSE_SLUG": "corpse_slug",
    "FAT_GREMLIN": "fat_gremlin",
    "FOSSIL_STALKER": "fossil_stalker",
    "INKLET": "inklet",
    "LAGAVULIN_MATRIARCH": "lagavulin_matriarch",
    "NIBBIT": "nibbit",
    "SNEAKY_GREMLIN": "sneaky_gremlin",
    "WATERFALL_GIANT": "waterfall_giant",
    "LIVING_SHIELD": "living_shield",
    "TURRET_OPERATOR": "turret_operator",
    "THE_LOST": "the_lost",
    "THE_FORGOTTEN": "the_forgotten",
    "AXEBOT": "axebot",
    "DEVOTED_SCULPTOR": "devoted_sculptor",
    "GLOBE_HEAD": "globe_head",
    "SCROLL_OF_BITING": "scroll_of_biting",
    "FROG_KNIGHT": "frog_knight",
    "FABRICATOR": "fabricator",
}


def read_pck_index(path: Path):
    data = path.read_bytes()
    assert data[:4] == b"GDPC"
    fmt = struct.unpack_from("<I", data, 4)[0]
    assert fmt >= 3
    off = 20
    flags = struct.unpack_from("<I", data, off)[0]
    off += 4
    file_base = struct.unpack_from("<Q", data, off)[0]
    off += 8
    dir_offset = struct.unpack_from("<Q", data, off)[0]
    if flags & 1:
        raise RuntimeError("encrypted directory")
    off = dir_offset
    file_count = struct.unpack_from("<I", data, off)[0]
    off += 4
    files: dict[str, tuple[int, int]] = {}
    for _ in range(file_count):
        slen = struct.unpack_from("<I", data, off)[0]
        off += 4
        name = data[off : off + slen].split(b"\0", 1)[0].decode("utf-8", "replace").replace("\\", "/")
        off += slen
        foff, fsize = struct.unpack_from("<QQ", data, off)
        off += 16 + 16 + 4
        files[name] = (file_base + foff, fsize)
    return data, files


def gst2_to_image(blob: bytes) -> Image.Image:
    if blob[:4] != b"GST2":
        raise ValueError("not GST2")
    # VRAM 圧縮（商人など）: ver/w/h … format@48, payload@52
    # Godot Image.FORMAT_DXT5 == 19
    if len(blob) > 52:
        ver, w, h = struct.unpack_from("<III", blob, 4)
        fmt = struct.unpack_from("<I", blob, 48)[0]
        bw, bh = (w + 3) // 4, (h + 3) // 4
        if fmt == 19 and len(blob) - 52 == bw * bh * 16:
            try:
                import texture2ddecoder  # type: ignore

                rgba = texture2ddecoder.decode_bc3(blob[52:], w, h)
                return Image.frombytes("RGBA", (w, h), rgba)
            except Exception:
                pass

    # header: ver,w,h,df,mipmap_limit,reserved*3
    off = 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4
    data_format, w16, h16, mipmaps, fmt = struct.unpack_from("<IHHII", blob, off)
    off += 4 + 2 + 2 + 4 + 4
    # DATA_FORMAT_PNG=0, WEBP=1? Our dump had 2 — accept PNG/WEBP magic in payload
    size = struct.unpack_from("<I", blob, off)[0]
    off += 4
    payload = blob[off : off + size]
    if payload[:4] == b"RIFF" or payload[:4] == b"WEBP":
        return Image.open(io.BytesIO(payload)).convert("RGBA")
    if payload[:8] == b"\x89PNG\r\n\x1a\n":
        return Image.open(io.BytesIO(payload)).convert("RGBA")
    # some webp start directly after RIFF already handled; try open anyway
    return Image.open(io.BytesIO(payload)).convert("RGBA")


def pascal_to_snake(name: str) -> str:
    s1 = re.sub("(.)([A-Z][a-z]+)", r"\1_\2", name)
    return re.sub("([a-z0-9])([A-Z])", r"\1_\2", s1).lower()


def list_hearts() -> list[tuple[str, str, str]]:
    out = []
    for path in HEARTS_DIR.glob("*Heart.cs"):
        text = path.read_text(encoding="utf-8")
        m = re.search(r"class\s+(\w+)\s*:\s*EnemyHeartRelic", text)
        if not m:
            continue
        mid = re.search(r'MonsterIdEntry\s*=>\s*"([^"]+)"', text)
        if not mid:
            continue
        cls = m.group(1)
        out.append((cls, pascal_to_snake(cls), mid.group(1)))
    return sorted(out)


def dump_visuals() -> dict[str, str]:
    r = subprocess.run(
        ["dotnet", "run", "--project", str(DUMP_PROJ / "DumpMonsterVisuals.csproj"), "-c", "Release", "--no-restore"],
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    if r.returncode != 0:
        r = subprocess.run(
            ["dotnet", "run", "--project", str(DUMP_PROJ / "DumpMonsterVisuals.csproj"), "-c", "Release"],
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
        )
    mapping = {}
    for line in r.stdout.splitlines():
        parts = line.strip().split("\t")
        if len(parts) >= 3 and parts[2].startswith("res://"):
            mapping[parts[0]] = Path(parts[2]).stem
    return mapping


def resolve_stem(entry: str, visuals: dict[str, str], atlas_stems: set[str]) -> str | None:
    if entry in STEM_ALIASES and STEM_ALIASES[entry] in atlas_stems:
        return STEM_ALIASES[entry]
    if entry in visuals and visuals[entry] in atlas_stems:
        return visuals[entry]
    # contains
    for k, stem in visuals.items():
        if entry in k or k in entry:
            if stem in atlas_stems:
                return stem
    soft = entry.lower()
    for stem in atlas_stems:
        if stem.replace("_", "") in soft.replace("_", "") or soft.replace("_", "") in stem.replace("_", ""):
            return stem
    # LEAF_SLIME_SMALL -> leaf_slime_s style
    cand = soft.replace("_small", "_s").replace("_med", "_m").replace("_medium", "_m")
    if cand in atlas_stems:
        return cand
    return STEM_ALIASES.get(entry)


def find_ctex(stem: str, files: dict[str, tuple[int, int]], data: bytes) -> str | None:
    """Resolve atlas PNG name via .import, then find matching GST2/WebP .ctex."""
    png_names: list[str] = [stem, stem.replace("_", "")]
    # animations/monsters/{stem}/ および backgrounds（商人など）の .png.import
    import_prefixes = (
        f"animations/monsters/{stem}/",
        f"animations/backgrounds/",
    )
    for n, (o, s) in files.items():
        if not n.endswith(".png.import"):
            continue
        under_monster = n.startswith(import_prefixes[0])
        under_bg = n.startswith(import_prefixes[1]) and n.endswith(f"/{stem}.png.import")
        if not (under_monster or under_bg):
            continue
        text = data[o : o + s].decode("utf-8", "replace")
        m = re.search(r'path="res://\.godot/imported/([^"]+\.ctex)"', text)
        if m:
            imported = ".godot/imported/" + m.group(1)
            if imported in files:
                return imported
        m2 = re.search(r'source_file="res://animations/[^"]+/([^"/]+\.png)"', text)
        if m2:
            png_names.append(Path(m2.group(1)).stem)

    variants = []
    for base in png_names:
        variants.extend([base, base.replace("_", ""), base.replace("_", "")])
    variants = list(dict.fromkeys(variants))

    cands: list[str] = []
    for n in files:
        if not n.endswith(".ctex"):
            continue
        low = n.lower()
        if any(f"{v}.png-" in low for v in variants):
            cands.append(n)
    if not cands:
        # last resort: any imported ctex containing stem compact form
        compact = stem.replace("_", "")
        cands = [n for n in files if n.endswith(".ctex") and compact in n.lower().replace("_", "")]

    if not cands:
        return None

    def score(n: str) -> tuple[int, int]:
        pen = 0
        if ".s3tc." in n:
            pen += 20
        if ".bptc." in n:
            pen += 20
        if "_outline" in n:
            pen += 5
        if "_particle" in n or "_blob" in n:
            pen += 8
        return (pen, len(n))

    cands.sort(key=score)
    return cands[0]


def find_spatlas(stem: str, files: dict[str, tuple[int, int]]) -> str | None:
    compact = stem.replace("_", "")
    cands = [n for n in files if n.endswith(".spatlas")]

    def pen(n: str) -> tuple[int, int]:
        base = n.split("/")[-1].lower()
        p = 0
        if "_back" in base or "_front" in base:
            p += 50
        if "phobia" in base:
            p += 80
        return (p, len(base))

    exact = [
        n
        for n in cands
        if f"/{stem}." in n or n.split("/")[-1].startswith(f"{stem}.atlas") or n.split("/")[-1].startswith(f"{stem}-")
    ]
    if exact:
        exact.sort(key=pen)
        return exact[0]

    # Word-order variants: tracker_ruby_raider ↔ ruby_raider_tracker
    tokens = [t for t in stem.split("_") if t]
    token_hits = []
    for n in cands:
        base = n.split("/")[-1].replace("_", "").replace("-", "").replace(".atlas", "").split(".")[0]
        if compact in base or all(t in base for t in tokens):
            token_hits.append(n)
    if token_hits:
        token_hits.sort(key=pen)
        return token_hits[0]
    return None


def parse_atlas_regions(atlas_data: str) -> list[tuple[str, int, int, int, int, bool]]:
    """Parse LibGDX/Spine atlas_data text → (name, x, y, w, h, rotated)."""
    lines = atlas_data.replace("\\n", "\n").splitlines()
    regions: list[tuple[str, int, int, int, int, bool]] = []
    i = 0
    while i < len(lines) and (
        not lines[i].strip() or ":" in lines[i] or lines[i].strip().endswith(".png")
    ):
        i += 1
    while i < len(lines):
        name = lines[i].strip()
        i += 1
        if not name:
            continue
        props: dict[str, str] = {}
        while i < len(lines) and (":" in lines[i] or not lines[i].strip()):
            if ":" in lines[i]:
                k, v = lines[i].split(":", 1)
                props[k.strip()] = v.strip()
            i += 1
        if "bounds" not in props:
            continue
        x, y, w, h = map(int, props["bounds"].split(","))
        rot_raw = props.get("rotate", "false").lower()
        rotated = rot_raw in ("true", "90", "y", "yes")
        regions.append((name, x, y, w, h, rotated))
    return regions


_JUNK = re.compile(
    r"(shadow|particle|blob|slash|vfx|drip|drop|dead_|swish|shine|effect|blur|_fx|seg_.*_fx|^layer\s*\d|add glows|brightness|ground grass)",
    re.I,
)


def score_region(name: str, w: int, h: int, skin_prefix: str | None) -> int:
    low = name.lower()
    if _JUNK.search(low):
        return -1
    if low.startswith("attack ") or low.startswith("attack_"):
        return -1
    area = w * h
    score = area
    base = low.split("/")[-1]
    if skin_prefix:
        if not low.startswith(skin_prefix.lower()):
            # Non-matching skin: only allow shared base parts with low priority
            if "/" in low:
                return -1
            score = area // 4
        else:
            score += 50_000
    # Prefer recognizable portrait pieces — but tiny “head*” chips lose to large body.
    # ハイライト／影バリアントは本体より優先しない（商人 head vs head_highlight など）。
    if "_highlight" in base or base.endswith("_s") or base.endswith(" stroke") or "_stroke" in base:
        score -= 80_000
    if base in ("head", "head-top", "head_top", "head top", "face", "skull"):
        score += 400_000 if area >= 800 else area  # tiny chips: don't over-prefer
    elif "head" in base:
        score += 180_000 if area >= 800 else area
    elif base in ("helmet", "helmet_top", "hood", "mask", "helmet_bottom"):
        score += 160_000
    elif "helmet" in base or "hood" in base or "mask" in base:
        score += 140_000
    elif "sac" in base or "flower" in base:
        score += 100_000
    elif base in ("bod", "body", "torso", "chestplate", "chest_plate"):
        score += 80_000
    elif "shriveled" in base or "boar" in low:
        score -= 40_000
    elif "cape" in base or "skirt" in base or "fur" in base:
        score -= 30_000
    elif "weapon" in base or "sword" in base or "arm" in base or "leg" in base or "foot" in base:
        score -= 20_000
    return score


def crop_region(atlas: Image.Image, region: tuple[str, int, int, int, int, bool]) -> Image.Image:
    _name, x, y, w, h, rotated = region
    # Spine/LibGDX atlas Y is from the top of the page
    crop = atlas.crop((x, y, x + w, y + h))
    if rotated:
        crop = crop.transpose(Image.Transpose.ROTATE_270)
    return crop.convert("RGBA")


def extract_portrait(
    atlas_img: Image.Image,
    regions: list[tuple[str, int, int, int, int, bool]],
    skin_prefix: str | None,
) -> tuple[Image.Image, str]:
    scored: list[tuple[int, tuple[str, int, int, int, int, bool]]] = []
    for r in regions:
        s = score_region(r[0], r[3], r[4], skin_prefix)
        if s > 0:
            scored.append((s, r))
    if not scored:
        # fallback: whole atlas trimmed
        return atlas_img, "(full-atlas)"
    scored.sort(key=lambda t: t[0], reverse=True)
    best = scored[0][1]
    return crop_region(atlas_img, best), best[0]


def make_icon(src: Image.Image, size: int) -> Image.Image:
    """Subject only on true alpha. No red ring / dark disc (vanilla relic style)."""
    img = src.convert("RGBA")
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)
    # Leave a little padding; do not clip to a circle (sticker frame looks wrong on dark UI).
    img.thumbnail((size - 12, size - 12), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    ox = (size - img.width) // 2
    oy = (size - img.height) // 2
    canvas.alpha_composite(img, (ox, oy))
    return canvas


def make_outline(icon: Image.Image) -> Image.Image:
    alpha = icon.split()[-1]
    mask = alpha.point(lambda a: 255 if a > 16 else 0)
    grown = mask.filter(ImageFilter.MaxFilter(5))
    out = Image.new("RGBA", icon.size, (0, 0, 0, 0))
    out.paste(Image.new("RGBA", icon.size, (255, 255, 255, 255)), mask=grown)
    return out


def main() -> None:
    print("Indexing PCK…")
    data, files = read_pck_index(PCK)
    atlas_stems = {
        n.split("/")[2]
        for n in files
        if n.startswith("animations/monsters/") and n.count("/") >= 2
    }
    print(f"atlas stems={len(atlas_stems)} files={len(files)}")

    print("Dumping monster visuals…")
    visuals = dump_visuals()
    print(f"visuals={len(visuals)}")

    hearts = list_hearts()
    OUT_BIG.mkdir(parents=True, exist_ok=True)
    report: list[str] = []
    ok = 0
    for cls, snake, mid in hearts:
        stem = resolve_stem(mid, visuals, atlas_stems)
        if not stem:
            report.append(f"MISS stem {cls} {mid}")
            continue
        ctex_name = find_ctex(stem, files, data)
        if not ctex_name:
            report.append(f"MISS ctex {cls} {mid} stem={stem}")
            continue
        o, s = files[ctex_name]
        blob = data[o : o + s]
        try:
            atlas_img = gst2_to_image(blob)
        except Exception as e:
            report.append(f"BAD decode {cls} {ctex_name}: {e}")
            continue

        skin = STEM_SKIN_PREFIX.get(mid)
        region_name = "(full-atlas)"
        spatlas_name = find_spatlas(stem, files)
        if spatlas_name:
            try:
                spatlas = json.loads(data[files[spatlas_name][0] : files[spatlas_name][0] + files[spatlas_name][1]])
                regions = parse_atlas_regions(spatlas.get("atlas_data", ""))
                src, region_name = extract_portrait(atlas_img, regions, skin)
            except Exception as e:
                src = atlas_img
                region_name = f"(atlas-fail:{e})"
        else:
            src = atlas_img

        icon = make_icon(src, ICON)
        big = make_icon(src, BIG)
        outline = make_outline(icon)
        icon.save(OUT_RELICS / f"{snake}.png")
        outline.save(OUT_RELICS / f"{snake}_outline.png")
        big.save(OUT_BIG / f"{snake}.png")
        ok += 1
        report.append(f"OK {cls} <- {stem}/{region_name} ({Path(ctex_name).name})")

    REPORT.write_text("\n".join(report) + "\n", encoding="utf-8")
    print(f"done OK={ok}/{len(hearts)}")
    for line in report:
        if not line.startswith("OK"):
            print(line)


if __name__ == "__main__":
    main()

