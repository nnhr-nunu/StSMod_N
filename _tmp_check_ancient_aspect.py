from PIL import Image
from pathlib import Path

# Check source image dimensions
orig = Path("D:/Dev/antigravity/StSMod_N/HypnosisCreator/images/card_portraits/original")
for name in ["107_2.png", "108_2.png", "108.jpg"]:
    p = orig / name
    if p.exists():
        img = Image.open(p)
        print(f"{name}: {img.size} aspect={img.size[0]/img.size[1]:.4f}")

# Check output portraits
port = Path("D:/Dev/antigravity/StSMod_N/HypnosisCreator/images/card_portraits")
for name in ["detox.png", "harmony.png"]:
    p = port / name
    if p.exists():
        img = Image.open(p)
        print(f"{name}: {img.size} aspect={img.size[0]/img.size[1]:.4f}")
