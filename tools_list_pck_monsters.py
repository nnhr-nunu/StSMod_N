# -*- coding: utf-8 -*-
"""List monster-related PNG/atlas paths inside SlayTheSpire2.pck."""
from __future__ import annotations

import struct
from pathlib import Path

PCK = Path(r"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck")
OUT = Path(r"D:\Dev\antigravity\StSMod_N\_pck_monster_paths.txt")


def read_index(data: bytes):
    assert data[:4] == b"GDPC"
    fmt_ver = struct.unpack_from("<I", data, 4)[0]
    off = 20
    if fmt_ver >= 2:
        off += 4  # flags
        off += 8  # file_base
        off += 16 * 4
    file_count = struct.unpack_from("<I", data, off)[0]
    off += 4
    files = []
    for _ in range(file_count):
        slen = struct.unpack_from("<I", data, off)[0]
        off += 4
        name = data[off : off + slen].split(b"\0", 1)[0].decode("utf-8", "replace")
        off += slen
        foff, fsize, rawsize = struct.unpack_from("<QQQ", data, off)
        off += 24
        off += 16  # md5
        if fmt_ver >= 2:
            off += 4  # flags
        files.append(name.replace("\\", "/"))
    return files


def main() -> None:
    data = PCK.read_bytes()
    files = read_index(data)
    keys = ("monster", "creature", "enemy", "spine", "visual")
    hits = [
        n
        for n in files
        if any(k in n.lower() for k in keys)
        and (n.lower().endswith((".png", ".atlas", ".tres", ".tscn", ".webp", ".svg")))
    ]
    # also broader: images/ with slime/gremlin etc
    names = (
        "slime",
        "gremlin",
        "raider",
        "cultist",
        "lagavulin",
        "byrd",
        "toad",
        "crab",
        "heart",
    )
    hits2 = [
        n
        for n in files
        if any(k in n.lower() for k in names)
        and n.lower().endswith((".png", ".atlas", ".webp"))
    ]
    all_hits = sorted(set(hits) | set(hits2))
    OUT.write_text("\n".join(all_hits) + f"\n\nTOTAL={len(all_hits)} of {len(files)}\n", encoding="utf-8")
    print(f"wrote {OUT} count={len(all_hits)} total_files={len(files)}")
    for n in all_hits[:40]:
        print(n)


if __name__ == "__main__":
    main()
