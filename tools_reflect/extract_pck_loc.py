import struct
import zlib
import sys

path = r"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\HypnosisCreator\HypnosisCreator.pck"
data = open(path, "rb").read()
assert data[:4] == b"GDPC"
fmt_ver, maj, minor, patch = struct.unpack_from("<IIII", data, 4)
print("format", fmt_ver, "godot", maj, minor, patch)
off = 20
pack_flags = 0
file_base = 0
if fmt_ver >= 2:
    pack_flags, = struct.unpack_from("<I", data, off)
    off += 4
    file_base, = struct.unpack_from("<Q", data, off)
    off += 8
    print("flags", pack_flags, "file_base", file_base)
    off += 16 * 4
file_count, = struct.unpack_from("<I", data, off)
off += 4
print("file_count", file_count)
files = []
for i in range(file_count):
    slen, = struct.unpack_from("<I", data, off)
    off += 4
    name = data[off : off + slen]
    off += slen
    name = name.split(b"\0", 1)[0].decode("utf-8", errors="replace")
    foff, fsize, rawsize = struct.unpack_from("<QQQ", data, off)
    off += 24
    off += 16  # md5
    flags = 0
    if fmt_ver >= 2:
        flags, = struct.unpack_from("<I", data, off)
        off += 4
    files.append((name, foff, fsize, rawsize, flags))

locs = [f for f in files if "localization" in f[0].lower()]
print("loc files", len(locs))
for n, fo, fs, rs, fl in locs:
    print(f"  {n} off={fo} size={fs} raw={rs} flags={fl}")

targets = [f for f in files if f[0].replace("\\", "/").endswith("localization/jpn/cards.json")]
print("targets", targets)
if not targets:
    for n, *_ in files:
        if "cards" in n or "localization" in n:
            print("?", n)
    sys.exit(1)

name, fo, fs, rs, fl = targets[0]
blob = data[fo : fo + fs]
text = None
try:
    text = blob.decode("utf-8")
except Exception:
    pass
if text is None and fs != rs:
    for w in (15, -15, 31, 47):
        try:
            text = zlib.decompress(blob, w).decode("utf-8")
            print("decompressed wbits", w)
            break
        except Exception:
            pass
if text is None:
    # Godot 4 may use Compression::MODE_ZSTD / DEFLATE etc via first bytes
    print("FAILED extract fs,rs", fs, rs, "head", blob[:64])
    # try zstd if available
    try:
        import zstandard as zstd

        text = zstd.ZstdDecompressor().decompress(blob, max_output_size=rs).decode("utf-8")
        print("zstd ok")
    except Exception as e:
        print("zstd fail", e)
        sys.exit(2)

print("LEN", len(text))
print(text[:600])
print("HAS BEGINNER", "BEGINNER_HYPNOSIS" in text)
print("HAS SLIME", "SLIME_HYPNOSIS" in text)
out = r"D:\Dev\antigravity\StSMod_N\tools_reflect\pck_jpn_cards.json"
open(out, "w", encoding="utf-8").write(text)
print("wrote", out)
