#!/usr/bin/env python3
"""Builds Assets/Resources/Vadronia/terrain-v3.png from terrain-v2.png.

The painted ground only has the two main roads and the plaza. This tool bakes in what makes the
houses belong to it: a back lane, dirt paths from every visible front door to the nearest road,
trampled yards behind the houses and a soft contact shade under each building. Houses are read from
TownLayout.cs, so the ground follows the layout; the dirt itself is sampled from the road texture.

    python3 Tools/build_terrain.py
"""
import random
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / "Tools"))
import preview_town as town  # noqa: E402  (parses TownLayout.cs)

RES = ROOT / "Assets/Resources/Vadronia"
SRC, OUT = RES / "terrain-v2.png", RES / "terrain-v3.png"
SS = 2                     # supersampling for smooth masks
ROADS = [4.95, -1.2, -8.2]  # centre lines of the E-W streets (north road, south road, back lane)


def to_px(x, y, w, h):
    return ((x + 14) / 28 * w, (11 - y) / 22 * h)


def dirt_tile(img):
    """Seamless dirt texture sampled from the south arm of the vertical road."""
    w, h = img.size
    x0, y0 = to_px(-.55, -4.2, w, h)
    x1, y1 = to_px(.35, -8.8, w, h)
    patch = img.crop((int(x0), int(y0), int(x1), int(y1)))
    top = Image.new("RGB", (patch.width * 2, patch.height))
    top.paste(patch, (0, 0))
    top.paste(patch.transpose(Image.FLIP_LEFT_RIGHT), (patch.width, 0))
    tile = Image.new("RGB", (top.width, top.height * 2))
    tile.paste(top, (0, 0))
    tile.paste(top.transpose(Image.FLIP_TOP_BOTTOM), (0, top.height))
    return tile


def tiled(tile, size, seed):
    rnd = random.Random(seed)
    out = Image.new("RGB", size)
    ox, oy = rnd.randint(0, tile.width), rnd.randint(0, tile.height)
    for y in range(-oy, size[1], tile.height):
        for x in range(-ox, size[0], tile.width):
            out.paste(tile, (x, y))
    return out


def soft_mask(shape_fn, size, seed, ragged=.2):
    """Returns a float mask 0..1 with an irregular, worn edge."""
    big = Image.new("L", (size[0] * SS, size[1] * SS), 0)
    shape_fn(ImageDraw.Draw(big))
    m = np.asarray(big.filter(ImageFilter.GaussianBlur(3 * SS)).resize(size, Image.LANCZOS), dtype=np.float32) / 255.0
    rnd = np.random.RandomState(seed)
    noise = rnd.rand(size[1] // 8 + 2, size[0] // 8 + 2).astype(np.float32)
    noise = np.asarray(Image.fromarray((noise * 255).astype(np.uint8)).resize(size, Image.BICUBIC).filter(ImageFilter.GaussianBlur(2)), dtype=np.float32) / 255.0
    return np.clip((m - .5 + (noise - .5) * ragged) * 6 + .5, 0, 1)


def polyline(points, width, size):
    w, h = size

    def draw(d):
        px = [tuple(c * SS for c in to_px(x, y, w, h)) for x, y in points]
        r = width / 2 * w / 28 * SS
        d.line(px, fill=255, width=int(r * 2), joint="curve")
        for x, y in px:
            d.ellipse([x - r, y - r, x + r, y + r], fill=255)
    return draw


def ellipse(cx, cy, rx, ry, size):
    w, h = size

    def draw(d):
        x, y = to_px(cx, cy, w, h)
        d.ellipse([(x - rx * w / 28) * SS, (y - ry * h / 22) * SS, (x + rx * w / 28) * SS, (y + ry * h / 22) * SS], fill=255)
    return draw


def door_x(p):
    """Horizontal door position of the front-facing sprites (measured on the artwork)."""
    flip = -1 if p["flip"] else 1
    if p["atlas"] == 0 and p["sprite"] == 0 or p["atlas"] == 1 and p["sprite"] in (2, 3):
        return p["x"] + flip * .18 * p["w"]
    if p["atlas"] == 0 and p["sprite"] == 3:
        return p["x"] - flip * .2 * p["w"]
    if p["atlas"] == 1 and p["sprite"] == 4:
        return p["x"] - flip * .02 * p["w"]
    return None


def main():
    img = Image.open(SRC).convert("RGB")
    size = img.size
    w, h = size
    tile = dirt_tile(img)
    arr = np.asarray(img, dtype=np.float32)
    props = town.parse_props()
    rnd = random.Random(11)
    jobs = []  # (mask, seed)

    def add_path(points, width, seed):
        jobs.append((soft_mask(polyline(points, width, size), size, seed), seed))

    def add_patch(cx, cy, rx, ry, seed):
        jobs.append((soft_mask(ellipse(cx, cy, rx, ry, size), size, seed, .3), seed))

    # --- back lane and the lane that leads to the south-east market stall
    add_path([(-13.6, -8.25), (-10, -8.15), (-6, -8.3), (-3, -8.2), (-.1, -8.25), (3, -8.15), (7, -8.3), (11, -8.2), (13.6, -8.25)], .95, 1)
    add_path([(.6, -5.0), (2.6, -5.15), (4.6, -4.95), (6.4, -4.95)], 1.0, 2)
    add_path([(6.4, -5.0), (7.3, -5.7), (8.0, -6.25)], .65, 3)
    # --- garden entrance and the stubs that connect the back yards to the south road
    add_path([(-11.05, 5.0), (-11.05, 6.2), (-10.7, 6.9), (-10.2, 7.25)], .8, 4)
    for i, x in enumerate((-8.0, -11.8, 12.4)):
        add_path([(x, -.15), (x + .1, -.45), (x, -.8)], .75, 20 + i)
        add_patch(x, -.1, 1.45, .45, 30 + i)
    add_patch(8.4, 1.7, 1.7, .6, 40)      # forge yard
    # --- a path from every visible front door to the nearest street
    for p in props:
        if p["flat"] or not p["solid"] or p["name"] in ("Estalagem", "Guilda — fachada"):
            continue
        dx = door_x(p)
        if dx is None:
            continue
        below = [r for r in ROADS if r < p["y"] - .3]
        if not below:
            continue
        road = max(below)
        j = rnd.uniform(-.25, .25)
        mid = (p["y"] + road) / 2
        add_patch(dx, p["y"] - .1, .85, .38, 100 + len(jobs))                      # worn apron at the doorstep
        add_path([(dx, p["y"] - .08), (dx + j, mid), (dx + j * .5, road + .05)], .85, 200 + len(jobs))
    # --- bake dirt
    dirt = np.asarray(tiled(tile, size, 5), dtype=np.float32)
    total = np.zeros(size[::-1], dtype=np.float32)
    for m, _ in jobs:
        total = np.maximum(total, m)
    out = arr * (1 - total[..., None]) + dirt * total[..., None]
    # darker worn rim and a few pebbles so the edge does not look cut out
    rim = np.clip(1 - np.abs(total - .45) * 3.2, 0, 1) * .16
    out *= (1 - rim)[..., None]
    pr = np.random.RandomState(9)
    ys, xs = np.where(total > .85)
    for i in pr.choice(len(xs), size=min(len(xs) // 90, 900), replace=False):
        v = pr.choice([.82, 1.15])
        out[ys[i]:ys[i] + 2, xs[i]:xs[i] + 3] *= v
    # --- contact shade so the houses sit in the ground instead of floating on it
    shade = np.zeros_like(total)
    for p in props:
        house = (p["atlas"] == 0 and p["sprite"] in (0, 1, 2, 3)) or (p["atlas"] == 1 and p["sprite"] <= 6)
        if not house or p["flat"]:
            continue
        shade = np.maximum(shade, soft_mask(ellipse(p["x"], p["y"] - .08, p["w"] * .56, .5, size), size, 300 + len(jobs), .05))
    shade = np.asarray(Image.fromarray((shade * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(5)), dtype=np.float32) / 255.0
    out *= (1 - .34 * shade)[..., None]
    Image.fromarray(np.clip(out, 0, 255).astype(np.uint8)).save(OUT, optimize=True)
    print(f"{OUT.name}: {w}x{h}, {len(jobs)} caminhos/pátios")


if __name__ == "__main__":
    main()
