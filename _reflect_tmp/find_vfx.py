from pathlib import Path
pck = Path(r'C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck')
data = pck.read_bytes()
for needle in [b'card_trail_', b'vfx_power_up']:
    idx = 0
    found=set()
    while True:
        i = data.find(needle, idx)
        if i < 0: break
        start=i
        while start>0 and 32 <= data[start-1] <= 126: start-=1
        end=i
        while end < len(data) and 32 <= data[end] <= 126: end+=1
        s=data[start:end].decode('ascii','ignore')
        if len(s)<140: found.add(s)
        idx=i+1
        if len(found)>50: break
    print('===', needle.decode(), '===')
    for s in sorted(found):
        print(s)
