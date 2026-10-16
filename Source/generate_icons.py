"""Draw original gene icons and the thrown-waste sprite. Requires Pillow."""
from pathlib import Path
import os
from PIL import Image, ImageDraw

dest = Path(__file__).resolve().parents[1] / 'Textures/UI/Genes'
dest.mkdir(parents=True, exist_ok=True)

def canvas():
    im = Image.new('RGBA', (512, 512))
    return im, ImageDraw.Draw(im)

im, d = canvas()
d.rounded_rectangle((32, 64, 480, 456), radius=38, fill=(53, 38, 29), outline=(219, 186, 139), width=12)
for y, edges in [(96, [(64, 238), (258, 448)]), (210, [(64, 140), (160, 338), (358, 448)]), (324, [(64, 238), (258, 448)])]:
    for x1, x2 in edges:
        d.rounded_rectangle((x1, y, x2, y+94), radius=15, fill=(158, 116, 67), outline=(205, 164, 108), width=5)
d.line([(326, 87), (297, 151), (328, 201), (291, 260), (306, 304)], fill=(61, 42, 29), width=17)
im.resize((128, 128), Image.Resampling.LANCZOS).save(dest / 'UG_FecalWalls.png')

im, d = canvas()
d.polygon([(256, 34), (113, 222), (78, 315), (99, 402), (169, 463), (256, 484), (343, 463), (413, 402), (434, 315), (399, 222)], fill=(96, 157, 182))
d.polygon([(256, 68), (139, 237), (112, 318), (130, 386), (187, 433), (256, 450), (325, 433), (382, 386), (400, 318), (373, 237)], fill=(237, 244, 236))
d.ellipse((147, 248, 192, 344), fill=(255, 255, 255))
d.rounded_rectangle((239, 293, 273, 403), radius=9, fill=(87, 147, 162))
d.rounded_rectangle((201, 331, 311, 365), radius=9, fill=(87, 147, 162))
im.resize((128, 128), Image.Resampling.LANCZOS).save(dest / 'UG_PermanentLactation.png')

im, d = canvas()
d.arc((50, 280, 462, 464), 2, 178, fill=(127, 219, 230), width=16)
d.arc((113, 310, 399, 426), 2, 178, fill=(219, 249, 247), width=14)
for x, y, s in [(256, 43, 1.0), (129, 125, 0.65), (383, 125, 0.65)]:
    points = [(x, y), (x-52*s, y+82*s), (x-59*s, y+116*s), (x-40*s, y+151*s),
              (x, y+165*s), (x+40*s, y+151*s), (x+59*s, y+116*s), (x+52*s, y+82*s)]
    d.polygon(points, fill=(204, 244, 247))
    d.ellipse((x-28*s, y+90*s, x-10*s, y+122*s), fill=(255, 255, 255))
im.resize((128, 128), Image.Resampling.LANCZOS).save(dest / 'UG_Squirter.png')
print(dest)

projectiles = Path(__file__).resolve().parents[1] / '1.7/Textures/Things/Projectile'
projectiles.mkdir(parents=True, exist_ok=True)
im = Image.new('RGBA', (64, 64))
d = ImageDraw.Draw(im)
d.ellipse((7, 20, 56, 51), fill=(68, 39, 18), outline=(39, 24, 12), width=3)
d.ellipse((16, 10, 47, 40), fill=(91, 54, 26))
d.arc((19, 18, 45, 37), 0, 160, fill=(116, 73, 35), width=3)
im.save(projectiles / 'Feces.png')

buildings = Path(__file__).resolve().parents[1] / '1.7/Textures/Things/Building'
buildings.mkdir(parents=True, exist_ok=True)
im = Image.new('RGBA', (128, 128))
d = ImageDraw.Draw(im)
d.rounded_rectangle((25, 25, 103, 108), radius=12, fill=(104, 112, 103), outline=(35, 43, 35), width=5)
d.ellipse((25, 14, 103, 48), fill=(144, 151, 136), outline=(35, 43, 35), width=5)
d.ellipse((36, 21, 92, 38), fill=(119, 126, 111), outline=(67, 76, 62), width=2)
d.rounded_rectangle((54, 17, 74, 26), radius=3, fill=(44, 51, 41))
d.line((27, 65, 101, 65), fill=(60, 70, 57), width=5)
d.line((27, 94, 101, 94), fill=(60, 70, 57), width=5)
im.save(buildings / 'SealedWasteContainer.png')
