"""Worker のラン日報で使う日本語名の表（server/src/names.json）を作る。

本家 SlayTheSpire2.pck と mod の localization/jpn から、カード・敵の出現（encounter）・イベントの名前を集める。
カードや敵を足したら実行し、server/ を push する（Workers Builds が自動でデプロイ）。
    python tools/gen_worker_names.py
"""
import json
import mmap
import os
import struct
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAME_PCK = r"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck"
MOD_LOC = os.path.join(ROOT, "HypnosisCreator", "localization", "jpn")
OUT = os.path.join(ROOT, "server", "src", "names.json")
# ID の種類（CARD.X の CARD）→ loc のファイル名
# Neow などの古の存在は EVENT.X だが、名前は ancients.json にある
KINDS = {"CARD": ["cards.json"], "ENCOUNTER": ["encounters.json"], "EVENT": ["events.json", "ancients.json"]}


def read_pck_files(path, wanted):
    """pck の中から wanted（末尾一致のパス）のファイルを取り出す。"""
    out = {}
    with open(path, "rb") as f, mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ) as data:
        assert data[:4] == b"GDPC"
        fmt_ver = struct.unpack_from("<I", data, 4)[0]
        off, file_base = 20, 0
        if fmt_ver >= 2:
            _flags, file_base = struct.unpack_from("<IQ", data, off)
            off += 12 + 16 * 4
        if fmt_ver >= 3:
            # Godot 4.4 以降は目次の位置がヘッダーにある
            off = struct.unpack_from("<Q", data, 32)[0]
        count = struct.unpack_from("<I", data, off)[0]
        off += 4
        for _ in range(count):
            slen = struct.unpack_from("<I", data, off)[0]
            off += 4
            name = bytes(data[off:off + slen]).split(b"\0", 1)[0].decode("utf-8", "replace")
            off += slen
            foff, fsize = struct.unpack_from("<QQ", data, off)
            off += 16 + 16 + (4 if fmt_ver >= 2 else 0)  # 位置・大きさ・md5・flags
            key = next((w for w in wanted if name.replace("\\", "/").endswith(w)), None)
            if key:
                blob = bytes(data[file_base + foff:file_base + foff + fsize])
                try:
                    out[key] = json.loads(blob.decode("utf-8-sig"))
                except (UnicodeDecodeError, json.JSONDecodeError):
                    out[key] = json.loads(zlib.decompress(blob).decode("utf-8-sig"))
    return out


def titles(loc):
    return {k[:-len(".title")]: v for k, v in loc.items() if k.endswith(".title") and isinstance(v, str)}


def main():
    wanted = [f"localization/jpn/{name}" for files in KINDS.values() for name in files]
    vanilla = read_pck_files(GAME_PCK, wanted)
    names = {}
    for kind, files in KINDS.items():
        merged = {}
        for file in files:
            merged.update(titles(vanilla.get(f"localization/jpn/{file}", {})))
            mod_path = os.path.join(MOD_LOC, file)
            if os.path.exists(mod_path):
                with open(mod_path, encoding="utf-8-sig") as f:
                    merged.update(titles(json.load(f)))
        for entry, title in merged.items():
            names[f"{kind}.{entry}"] = title
        print(kind, len(merged))
    with open(OUT, "w", encoding="utf-8", newline="\n") as f:
        json.dump(dict(sorted(names.items())), f, ensure_ascii=False, indent=0)
        f.write("\n")
    print("wrote", OUT, len(names))


if __name__ == "__main__":
    main()
