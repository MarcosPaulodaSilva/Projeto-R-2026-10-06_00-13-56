#!/usr/bin/env python3
"""Builds Assets/Resources/Vadronia/town-extra.png from the original town atlas.

Everything here is derived from `town.png` (back views, recoloured roofs) or drawn
procedurally in the same palette (barrels, crates, fences...). No external art.

Run from the repository root:  python3 Tools/build_town_extra.py
Prints the C# frame table to paste into Assets/Scripts/Visual/TownAtlasLayout.cs.
"""
import colorsys
import random
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "Assets/Resources/Vadronia/town.png"
OUT = ROOT / "Assets/Resources/Vadronia/town-extra.png"

# Same rectangles as TownAtlasLayout.Frames (Unity: origin bottom-left).
TOWN = [(15, 525, 310, 456), (349, 524, 427, 467)]

STONE = [(196, 192, 178), (172, 168, 154), (208, 204, 190), (154, 150, 138), (184, 178, 160)]
MORTAR = (104, 100, 90)
OUTLINE = (38, 24, 16)


def town_crop(i):
    im = Image.open(SRC).convert("RGBA")
    rx, ry, rw, rh = TOWN[i]
    top = im.height - (ry + rh)
    return im.crop((rx, top, rx + rw, top + rh))


def masonry(img, box, seed):
    """Fills `box` with irregular stone courses (same palette as the tavern base)."""
    rnd = random.Random(seed)
    d = ImageDraw.Draw(img)
    x0, y0, x1, y1 = box
    y = y0
    row = 0
    while y < y1:
        h = rnd.randint(15, 18)
        x = x0 - (row % 2) * rnd.randint(8, 14)
        while x < x1:
            w = rnd.randint(22, 34)
            col = rnd.choice(STONE)
            jitter = rnd.randint(-6, 6)
            col = tuple(max(0, min(255, c + jitter)) for c in col)
            xa, xb = max(x, x0), min(x + w, x1)
            ya, yb = y, min(y + h, y1)
            if xb > xa:
                d.rectangle([xa, ya, xb - 1, yb - 1], fill=col)
                d.rectangle([xa, ya, xb - 1, ya], fill=tuple(min(255, c + 14) for c in col))
                d.rectangle([xa, yb - 1, xb - 1, yb - 1], fill=MORTAR)
                d.rectangle([xb - 1, ya, xb - 1, yb - 1], fill=MORTAR)
            x += w
        y += h
        row += 1


def plaster(img, box, base, seed):
    rnd = random.Random(seed)
    x0, y0, x1, y1 = box
    px = img.load()
    for y in range(y0, y1):
        for x in range(x0, x1):
            n = rnd.randint(-7, 7)
            px[x, y] = (base[0] + n, base[1] + n, base[2] + n, 255)


def erase(img, box):
    d = ImageDraw.Draw(img)
    d.rectangle(box, fill=(0, 0, 0, 0))


def median_color(img, box):
    arr = np.array(img.crop(box).convert("RGB")).reshape(-1, 3)
    return tuple(int(v) for v in np.median(arr, axis=0))


def back_cottage():
    """Cottage seen from behind: the window half is mirrored over the door half."""
    im = town_crop(0)
    stone = im.crop((100, 410, 143, 436))               # clean foundation stone
    post = im.crop((28, 372, 40, 400))                  # clean timber post
    mid = im.crop((148, 340, 162, 368))                 # clean mid post, no pot leaves
    plaster_strip = im.crop((142, 352, 148, 368))
    for box in [(0, 340, 27, 456), (276, 340, 310, 456), (0, 408, 62, 456),
                (62, 436, 310, 456), (140, 400, 182, 456)]:
        erase(im, box)          # bushes, barrel, steps, pots
    for y in (385, 401):
        im.alpha_composite(plaster_strip, (142, y))     # pot leaves over the plaster
    for x in (41, 84, 127):
        im.alpha_composite(stone, (x, 410))
    for y in (400, 428):
        im.alpha_composite(post, (28, y))
    for y in (372, 400, 428):
        im.alpha_composite(mid, (148, y))
    src = im.crop((28, 292, 150, 437)).transpose(Image.FLIP_LEFT_RIGHT)
    erase(im, (160, 292, 283, 437))
    im.alpha_composite(src, (160, 292))
    return im.crop((0, 0, 310, 438))


def back_inn():
    """Tavern from behind: stone wall without door, lamps or sign; extra arched window."""
    im = town_crop(1)
    arch = im.crop((50, 322, 130, 416))
    for box in [(0, 360, 58, 467), (338, 380, 427, 467), (380, 225, 427, 315)]:
        erase(im, box)
    masonry(im, (30, 322, 380, 456), 21)
    im.alpha_composite(arch, (50, 322))
    right = town_crop(1).crop((282, 322, 352, 416))
    im.alpha_composite(right, (282, 322))
    im.alpha_composite(arch, (163, 322))
    # Roof overhang row stays untouched above y=322.
    return im.crop((0, 0, 427, 456))


def recolor_roof(img, roof_bottom, hue_from, hue_to_deg, sat_mul=1.0, val_mul=1.0, box_x=None):
    arr = np.array(img.convert("RGBA")).astype(np.float32) / 255.0
    h, w, _ = arr.shape
    out = arr.copy()
    for y in range(0, min(roof_bottom, h)):
        for x in range(w):
            r, g, b, a = arr[y, x]
            if a < .1:
                continue
            hh, ss, vv = colorsys.rgb_to_hsv(r, g, b)
            if ss > .30 and hue_from[0] <= hh * 360 <= hue_from[1]:
                nr, ng, nb = colorsys.hsv_to_rgb(hue_to_deg / 360.0, min(1, ss * sat_mul), min(1, vv * val_mul))
                out[y, x] = (nr, ng, nb, a)
    return Image.fromarray((out * 255).astype(np.uint8), "RGBA")


# ---------------------------------------------------------------- procedural props
def shade(c, f):
    return tuple(max(0, min(255, int(v * f))) for v in c)


def outlined(img):
    """Adds a 1px dark outline around the opaque area, like the original sprites."""
    alpha = img.split()[3]
    grown = alpha.filter(ImageFilter.MaxFilter(3))
    edge = Image.new("RGBA", img.size, OUTLINE + (255,))
    mask = Image.eval(grown, lambda v: 255 if v > 40 else 0)
    base = Image.new("RGBA", img.size, (0, 0, 0, 0))
    base.paste(edge, (0, 0), mask)
    base.alpha_composite(img)
    return base


def ss_canvas(w, h, scale=3):
    return Image.new("RGBA", (w * scale, h * scale), (0, 0, 0, 0)), scale


def finish(img, scale):
    small = img.resize((img.width // scale, img.height // scale), Image.LANCZOS)
    return outlined(small)


def wood_planks(d, box, base, scale, vertical=True, seed=1):
    rnd = random.Random(seed)
    x0, y0, x1, y1 = [v * scale for v in box]
    d.rectangle([x0, y0, x1, y1], fill=base)
    if vertical:
        x = x0
        while x < x1:
            w = rnd.randint(7, 11) * scale // 2
            d.rectangle([x, y0, min(x + w, x1), y1], fill=shade(base, rnd.uniform(.9, 1.1)))
            d.line([(x, y0), (x, y1)], fill=shade(base, .62), width=max(1, scale - 1))
            x += w
    else:
        y = y0
        while y < y1:
            h = rnd.randint(7, 11) * scale // 2
            d.rectangle([x0, y, x1, min(y + h, y1)], fill=shade(base, rnd.uniform(.9, 1.1)))
            d.line([(x0, y), (x1, y)], fill=shade(base, .62), width=max(1, scale - 1))
            y += h


def barrel():
    w, h = 54, 66
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = (150, 98, 52)
    d.ellipse([3 * s, 50 * s, 51 * s, 64 * s], fill=shade(wood, .7))
    d.polygon([(5 * s, 16 * s), (49 * s, 16 * s), (53 * s, 38 * s), (49 * s, 57 * s), (5 * s, 57 * s), (1 * s, 38 * s)], fill=wood)
    for x in range(8, 48, 6):
        d.line([(x * s, 17 * s), ((x + (x - 27) // 14) * s, 57 * s)], fill=shade(wood, .72), width=s)
    d.rectangle([2 * s, 24 * s, 52 * s, 28 * s], fill=(92, 98, 108))
    d.rectangle([2 * s, 46 * s, 52 * s, 50 * s], fill=(92, 98, 108))
    d.rectangle([2 * s, 24 * s, 52 * s, 25 * s], fill=(150, 156, 166))
    d.ellipse([5 * s, 8 * s, 49 * s, 25 * s], fill=shade(wood, 1.15))
    d.ellipse([9 * s, 11 * s, 45 * s, 22 * s], fill=shade(wood, .9))
    return finish(img, s)


def crates():
    w, h = 96, 84
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = (172, 124, 70)

    def crate(x, y, cw, ch, seed):
        wood_planks(d, (x, y, x + cw, y + ch), wood, s, vertical=False, seed=seed)
        d.rectangle([x * s, y * s, (x + cw) * s, (y + 5) * s], fill=shade(wood, 1.12))
        d.rectangle([x * s, y * s, (x + 5) * s, (y + ch) * s], fill=shade(wood, .8))
        d.rectangle([(x + cw - 5) * s, y * s, (x + cw) * s, (y + ch) * s], fill=shade(wood, .8))
        d.line([(x * s, y * s), ((x + cw) * s, (y + ch) * s)], fill=shade(wood, .65), width=s * 2)
    crate(4, 38, 44, 40, 3)
    crate(50, 44, 42, 36, 5)
    crate(20, 4, 42, 38, 9)
    # a few apples spilling out of the top crate
    for cx, cy, col in [(30, 8, (192, 52, 40)), (42, 6, (206, 70, 44)), (52, 10, (170, 44, 36))]:
        d.ellipse([(cx - 5) * s, (cy - 3) * s, (cx + 5) * s, (cy + 6) * s], fill=col)
    return finish(img, s)


def hay():
    w, h = 84, 62
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    straw = (214, 178, 78)
    d.rounded_rectangle([2 * s, 8 * s, 82 * s, 60 * s], radius=10 * s, fill=straw)
    d.rounded_rectangle([2 * s, 8 * s, 82 * s, 24 * s], radius=10 * s, fill=shade(straw, 1.15))
    rnd = random.Random(4)
    for _ in range(90):
        x, y = rnd.randint(6, 78), rnd.randint(12, 56)
        d.line([(x * s, y * s), ((x + rnd.randint(-6, 6)) * s, (y + rnd.randint(2, 7)) * s)],
               fill=shade(straw, rnd.choice([.72, .85, 1.2])), width=s)
    for x in (24, 58):
        d.rectangle([x * s, 8 * s, (x + 3) * s, 60 * s], fill=(142, 96, 52))
    return finish(img, s)


def fence():
    w, h = 118, 60
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = (148, 100, 56)
    for x in (4, 54, 104):
        wood_planks(d, (x, 6, x + 10, 58), wood, s, vertical=True, seed=x)
        d.polygon([(x * s, 6 * s), ((x + 5) * s, 0), ((x + 10) * s, 6 * s)], fill=shade(wood, 1.1))
    for y in (18, 38):
        wood_planks(d, (0, y, w - 2, y + 8), shade(wood, 1.08), s, vertical=False, seed=y)
        d.rectangle([0, y * s, (w - 2) * s, (y + 2) * s], fill=shade(wood, 1.25))
    return finish(img, s)


def bench():
    w, h = 104, 62
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = (156, 108, 62)
    for x in (8, 88):
        wood_planks(d, (x, 28, x + 8, 58), shade(wood, .8), s, vertical=True, seed=x)
    wood_planks(d, (2, 4, 100, 16), wood, s, vertical=False, seed=2)   # backrest
    wood_planks(d, (0, 28, 102, 40), shade(wood, 1.1), s, vertical=False, seed=6)  # seat
    d.rectangle([0, 28 * s, 102 * s, 30 * s], fill=shade(wood, 1.3))
    for x in (12, 90):
        wood_planks(d, (x, 14, x + 4, 30), shade(wood, .75), s, vertical=True, seed=x + 1)
    return finish(img, s)


def notice_board():
    w, h = 96, 118
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = (142, 94, 52)
    for x in (10, 78):
        wood_planks(d, (x, 40, x + 8, 114), shade(wood, .85), s, vertical=True, seed=x)
    wood_planks(d, (2, 4, 94, 62), wood, s, vertical=True, seed=8)
    d.rectangle([2 * s, 4 * s, 94 * s, 8 * s], fill=shade(wood, 1.25))
    d.polygon([(0, 4 * s), (48 * s, -6 * s), (96 * s, 4 * s), (96 * s, 12 * s), (0, 12 * s)], fill=(164, 56, 40))
    for (x, y, pw, ph, c) in [(10, 18, 24, 30, (236, 224, 190)), (40, 16, 22, 26, (222, 208, 170)),
                              (66, 22, 22, 32, (240, 230, 200)), (22, 40, 20, 16, (210, 196, 160))]:
        d.rectangle([x * s, y * s, (x + pw) * s, (y + ph) * s], fill=c)
        for i in range(3, ph - 3, 5):
            d.line([((x + 3) * s, (y + i) * s), ((x + pw - 3) * s, (y + i) * s)], fill=(120, 96, 70), width=s)
        d.ellipse([(x + pw // 2 - 2) * s, (y + 1) * s, (x + pw // 2 + 2) * s, (y + 5) * s], fill=(170, 40, 40))
    return finish(img, s)


def flowerbed():
    w, h = 118, 52
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    soil = (96, 64, 40)
    d.rounded_rectangle([2 * s, 10 * s, 116 * s, 50 * s], radius=14 * s, fill=(130, 116, 96))   # stone rim
    d.rounded_rectangle([7 * s, 14 * s, 111 * s, 45 * s], radius=11 * s, fill=soil)
    rnd = random.Random(12)
    cols = [(226, 60, 52), (244, 196, 52), (236, 120, 190), (250, 246, 232), (120, 150, 255)]
    for _ in range(46):
        x, y = rnd.randint(14, 104), rnd.randint(17, 42)
        d.ellipse([(x - 6) * s, (y - 5) * s, (x + 6) * s, (y + 5) * s], fill=(58, 128, 52))
        c = rnd.choice(cols)
        d.ellipse([(x - 3) * s, (y - 6) * s, (x + 3) * s, (y) * s], fill=c)
    return finish(img, s)


def cart():
    w, h = 150, 104
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = (150, 102, 58)
    wood_planks(d, (14, 34, 128, 74), wood, s, vertical=False, seed=14)
    d.polygon([(20 * s, 34 * s), (122 * s, 34 * s), (112 * s, 20 * s), (30 * s, 20 * s)], fill=(222, 186, 84))   # hay load
    rnd = random.Random(5)
    for _ in range(70):
        x, y = rnd.randint(24, 118), rnd.randint(22, 36)
        d.line([(x * s, y * s), ((x + rnd.randint(-5, 5)) * s, (y - rnd.randint(2, 6)) * s)],
               fill=shade((222, 186, 84), rnd.choice([.75, 1.15])), width=s)
    wood_planks(d, (126, 58, 148, 64), shade(wood, .9), s, vertical=False, seed=2)   # handle
    d.ellipse([34 * s, 54 * s, 78 * s, 100 * s], fill=(88, 58, 34))
    d.ellipse([40 * s, 60 * s, 72 * s, 94 * s], fill=(138, 96, 56))
    d.ellipse([52 * s, 72 * s, 60 * s, 82 * s], fill=(70, 46, 28))
    for a in range(0, 180, 45):
        import math
        dx, dy = math.cos(math.radians(a)) * 15, math.sin(math.radians(a)) * 17
        d.line([((56 - dx) * s, (77 - dy) * s), ((56 + dx) * s, (77 + dy) * s)], fill=(88, 58, 34), width=s * 2)
    return finish(img, s)


def dirt_patch():
    """Flat decal: worn earth path used to join door steps to the street."""
    w, h = 130, 90
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    rnd = random.Random(31)
    px = img.load()
    cx, cy = w / 2, h / 2
    for y in range(h):
        for x in range(w):
            dx, dy = (x - cx) / (w / 2), (y - cy) / (h / 2)
            r = (dx * dx + dy * dy) ** .5 + rnd.uniform(-.1, .1)
            if r < 1:
                n = rnd.randint(-6, 6)
                a = int(255 * min(1, (1 - r) * 3.2))
                px[x, y] = (166 + n, 138 + n, 90 + n, a)
    return img


def tidy(img):
    box = img.getbbox()
    return img.crop(box) if box else img


def hue_variants():
    cottage = town_crop(0)
    inn = town_crop(1)
    return {
        "cottage_blue": recolor_roof(cottage, 222, (0, 40), 214, .62, .8),
        "cottage_green": recolor_roof(cottage, 222, (0, 40), 112, .5, .68),
        "inn_red": recolor_roof(inn, 200, (200, 260), 8, 1.9, 1.5),
    }


def main():
    sprites = []  # (name, image)
    cottage_back = back_cottage()
    inn_back = back_inn()
    variants = hue_variants()
    sprites += [
        ("cottage_back", cottage_back),
        ("inn_back", inn_back),
        ("cottage_blue", variants["cottage_blue"]),
        ("cottage_green", variants["cottage_green"]),
        ("inn_red", variants["inn_red"]),
        ("cottage_back_blue", recolor_roof(cottage_back, 222, (0, 40), 214, .62, .8)),
        ("inn_back_red", recolor_roof(inn_back, 200, (200, 260), 8, 1.9, 1.5)),
        ("barrel", barrel()), ("crates", crates()), ("hay", hay()), ("fence", fence()),
        ("bench", bench()), ("notice_board", notice_board()), ("flowerbed", flowerbed()),
        ("cart", cart()), ("dirt_patch", dirt_patch()),
    ]
    sprites = [(n, tidy(i)) for n, i in sprites]
    # Shelf packing into a 2048-wide sheet, 8px gutters (point filtering, no bleeding).
    sheet_w, gutter = 2048, 8
    x = y = row_h = 0
    placed = []
    for name, im in sprites:
        if x + im.width + gutter > sheet_w:
            x, y, row_h = 0, y + row_h + gutter, 0
        placed.append((name, im, x, y))
        x += im.width + gutter
        row_h = max(row_h, im.height)
    sheet_h = y + row_h + gutter
    sheet = Image.new("RGBA", (sheet_w, sheet_h), (0, 0, 0, 0))
    for name, im, px, py in placed:
        sheet.alpha_composite(im, (px, py))
    sheet.save(OUT)
    print(f"// {OUT.name}: {sheet_w}x{sheet_h}")
    for i, (name, im, px, py) in enumerate(placed):
        print(f"            new Rect({px}, {sheet_h - py - im.height}, {im.width}, {im.height}), // {i} {name}")


if __name__ == "__main__":
    main()
