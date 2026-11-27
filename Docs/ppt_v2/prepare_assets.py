import os
from PIL import Image

ASSETS = r"e:\1FairyOnTheTree\The-realm-of-resonance\Docs\ppt_v2\assets"
OUT = r"e:\1FairyOnTheTree\The-realm-of-resonance\Docs\ppt_v2\prepared"
os.makedirs(OUT, exist_ok=True)

# 1. cover_map_bleed.png: 920 x 1080 center crop from map_overview.png (1281x716)
im = Image.open(os.path.join(ASSETS, "map_overview.png"))
w, h = im.size  # 1281 x 716
target_ratio = 920 / 1080  # 0.85185
new_w = int(round(h * target_ratio))  # ~610
left = (w - new_w) // 2
im_bleed = im.crop((left, 0, left + new_w, h)).resize((920, 1080), Image.Resampling.LANCZOS)
im_bleed.save(os.path.join(OUT, "cover_map_bleed.png"))

# 2. ai_map_crop.png: 1100 x 840 focused on town core (removing outer ocean margins)
crop_box = (int(w * 0.06), int(h * 0.14), int(w * 0.94), int(h * 0.94))
im_town = im.crop(crop_box)
tw, th = im_town.size
t_ratio = 1100 / 840
if tw / th > t_ratio:
    cw = int(th * t_ratio)
    cl = (tw - cw) // 2
    im_town = im_town.crop((cl, 0, cl + cw, th))
else:
    ch = int(tw / t_ratio)
    ct = (th - ch) // 2
    im_town = im_town.crop((0, ct, tw, ct + ch))
im_town.resize((1100, 840), Image.Resampling.LANCZOS).save(os.path.join(OUT, "ai_map_crop.png"))

print("Prepared:", os.listdir(OUT))
