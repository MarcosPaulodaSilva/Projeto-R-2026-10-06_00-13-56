#!/usr/bin/env python3
"""Offline preview of the Grünwald layout, parsed straight from TownLayout.cs.

Mirrors TownWorld: ground scaled to 28x22 units, sprites scaled by width with a
bottom-centre pivot, sorting by -Y, soft shadows and the forest border. Also ports
MovementCore.Move to check Conrad's patrol and the spawn/herb points against collisions.

    python3 Tools/preview_town.py [out.png] [--blocks]
"""
import math
import re
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageOps

ROOT = Path(__file__).resolve().parent.parent
RES = ROOT / "Assets/Resources/Vadronia"
LAYOUT = (ROOT / "Assets/Scripts/World/TownLayout.cs").read_text(encoding="utf-8")
ATLAS = (ROOT / "Assets/Scripts/Visual/TownAtlasLayout.cs").read_text(encoding="utf-8")
PPU = 40  # preview pixels per world unit
HALF_W, HALF_H = 14, 11


def rects(block_name):
    body = re.search(block_name + r"\s*=\s*\{(.*?)\};", ATLAS, re.S).group(1)
    return [tuple(int(v) for v in m) for m in re.findall(r"new Rect\((\d+),\s*(\d+),\s*(\d+),\s*(\d+)\)", body)]


TOWN, EXTRA = rects("Frames"), rects("Extra")
SHEETS = {0: Image.open(RES / "town.png").convert("RGBA"), 1: Image.open(RES / "town-extra.png").convert("RGBA")}
FRAMES = {0: TOWN, 1: EXTRA}


def num(token):
    token = token.strip()
    if token in ("true", "false"):
        return token == "true"
    return float(token.rstrip("f"))


def parse_props():
    props = []
    for m in re.finditer(r'(Town|Extra|Soft)\("([^"]+)",\s*([^)]*)\)(?:\.Tinted\(([^)]*)\))?', LAYOUT):
        kind, name, args, tint = m.groups()
        a = [num(t) for t in args.split(",")]
        p = dict(name=name, tint=tuple(num(t) for t in tint.split(",")) if tint else (1, 1, 1))
        if kind == "Soft":
            p.update(atlas=1, sprite=int(a[0]), x=a[1], y=a[2], w=a[3], d=0, flip=bool(a[4]) if len(a) > 4 else False, solid=False, flat=True)
        else:
            p.update(atlas=0 if kind == "Town" else 1, sprite=int(a[0]), x=a[1], y=a[2], w=a[3], d=a[4],
                     flip=bool(a[5]) if len(a) > 5 else False, solid=True, flat=False)
        props.append(p)
    return props


def footprint(p):
    tree = p["atlas"] == 0 and p["sprite"] in (4, 5)
    if tree:
        return (p["x"] - .22, p["y"], p["x"] + .22, p["y"] + .35)
    k = .38 if p["atlas"] == 0 else .42
    return (p["x"] - p["w"] * k, p["y"], p["x"] + p["w"] * k, p["y"] + p["d"])


def parse_patrol():
    body = re.search(r"Patrol\s*=\s*\{(.*?)\};", LAYOUT, re.S).group(1)
    pts = [(num(a), num(b)) for a, b in re.findall(r"new FootPoint\(([^,]+),\s*([^)]+)\)", body)]
    return pts


# ---- MovementCore.Move port
RADIUS = .22


def contains(b, x, y):
    nx, ny = max(b[0], min(b[2], x)), max(b[1], min(b[3], y))
    return (x - nx) ** 2 + (y - ny) ** 2 < RADIUS ** 2


def clear(x, y, blocks):
    return not any(contains(b, x, y) for b in blocks)


def move(pt, dx, dy, blocks):
    steps = max(1, math.ceil(max(abs(dx), abs(dy)) / .08))
    x, y = pt
    for _ in range(steps):
        nx = max(-13.5 + RADIUS, min(13.5 - RADIUS, x + dx / steps))
        if clear(nx, y, blocks):
            x = nx
        ny = max(-10.5 + RADIUS, min(10.5 - RADIUS, y + dy / steps))
        if clear(x, ny, blocks):
            y = ny
    return (x, y)


def check_patrol(patrol, blocks):
    issues = []
    p = patrol[0]
    target, legs = 1, 0
    for _ in range(60 * 600):
        t = patrol[target]
        dx, dy = t[0] - p[0], t[1] - p[1]
        d = math.hypot(dx, dy)
        step = min(d, 1.35 / 60)
        if d > 0:
            p = move(p, dx / d * step, dy / d * step, blocks)
        if abs(p[0] - t[0]) + abs(p[1] - t[1]) < .02:
            legs += 1
            target = (target + 1) % len(patrol)
        if legs > len(patrol) * 3:
            return issues, legs
        if not clear(p[0], p[1], blocks):
            issues.append(f"Conrad dentro de obstáculo perto de {p}")
            return issues, legs
    issues.append(f"Patrulha presa a caminho do ponto {target} {patrol[target]} (posição {tuple(round(v, 2) for v in p)})")
    return issues, legs


def shadow(size):
    w, h = max(2, int(size[0])), max(2, int(size[1]))
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).ellipse([w * .08, h * .08, w * .92, h * .92], fill=int(255 * .7))
    return mask.filter(ImageFilter.GaussianBlur(max(1, h // 6)))


def sprite_of(p):
    x, y, w, h = FRAMES[p["atlas"]][p["sprite"]]
    sheet = SHEETS[p["atlas"]]
    im = sheet.crop((x, sheet.height - (y + h), x + w, sheet.height - y))
    if p["flip"]:
        im = ImageOps.mirror(im)
    scale = p["w"] / w * PPU
    im = im.resize((max(1, int(w * scale)), max(1, int(h * scale))), Image.LANCZOS)
    r, g, b = p["tint"]
    if (r, g, b) != (1, 1, 1):
        px = im.load()
        for yy in range(im.height):
            for xx in range(im.width):
                pr, pg, pb, pa = px[xx, yy]
                px[xx, yy] = (int(pr * r), int(pg * g), int(pb * b), pa)
    return im


def to_px(x, y):
    return int((x + HALF_W) * PPU), int((HALF_H - y) * PPU)


def render(show_blocks=False):
    props = parse_props()
    blocks = [footprint(p) for p in props if p["solid"]]
    patrol = parse_patrol()
    W, H = HALF_W * 2 * PPU, HALF_H * 2 * PPU
    ground = Image.open(RES / "terrain-v2.png").convert("RGBA").resize((W, H), Image.LANCZOS)
    canvas = Image.new("RGBA", (W, H), (34, 48, 35, 255))
    canvas.alpha_composite(ground)
    # flat decals first, then shadows, then everything sorted far -> near
    for p in props:
        if p["flat"]:
            im = sprite_of(p)
            px, py = to_px(p["x"], p["y"])
            canvas.alpha_composite(im, (px - im.width // 2, py - im.height // 2))
    drawables = []
    for p in props:
        if p["flat"]:
            continue
        drawables.append((p["y"], "prop", p))
    sc = TOWN
    for i in range(28):
        t = 4 if i % 3 == 0 else 5
        w = 2.2 + (i % 4) * .23
        x = (-13.8 + (i % 2) * .4) if i < 14 else (13.8 - (i % 2) * .4)
        y = -10.7 + (i % 14) * 1.65
        drawables.append((y, "prop", dict(atlas=0, sprite=t, x=x, y=y, w=w, d=.4, flip=False, tint=(1, 1, 1), name="Bosque", solid=False, flat=False)))
    for _, _, p in drawables:
        px, py = to_px(p["x"] + .3, p["y"] - .12)
        sw, sh = p["w"] * 1.15 * PPU, p["w"] * .42 * PPU
        sh_mask = shadow((sw, sh))
        layer = Image.new("RGBA", sh_mask.size, (18, 26, 19, 255))
        layer.putalpha(sh_mask)
        canvas.alpha_composite(layer, (int(px - sh_mask.width / 2), int(py - sh_mask.height / 2)))
    for _, _, p in sorted(drawables, key=lambda d: -d[0]):
        im = sprite_of(p)
        px, py = to_px(p["x"], p["y"])
        canvas.alpha_composite(im, (px - im.width // 2, py - im.height))
    d = ImageDraw.Draw(canvas)
    for lx, ly in [(-4.5, -3.7), (4.5, -3.7), (-4.6, 3.8), (4.6, 3.8)]:
        a = to_px(lx, ly)
        d.line([a, (a[0], a[1] - int(1.2 * PPU))], fill=(56, 38, 23), width=3)
        d.ellipse([a[0] - 5, a[1] - int(1.2 * PPU) - 6, a[0] + 5, a[1] - int(1.2 * PPU) + 6], fill=(242, 174, 72))
    for hx, hy in [(-5.5, -2), (8, -6.5), (-10.2, 7.2)]:
        a = to_px(hx, hy)
        d.ellipse([a[0] - 9, a[1] - 9, a[0] + 9, a[1] + 9], outline=(255, 255, 80), width=3)
    a = to_px(0, -3.9)
    d.ellipse([a[0] - 7, a[1] - 7, a[0] + 7, a[1] + 7], fill=(60, 140, 255))
    if show_blocks:
        for b in blocks:
            (x0, y0), (x1, y1) = to_px(b[0], b[3]), to_px(b[2], b[1])
            d.rectangle([x0, y0, x1, y1], outline=(255, 70, 70), width=2)
        pts = [to_px(*q) for q in patrol]
        d.line(pts + [pts[0]], fill=(255, 140, 0), width=3)
        for i, q in enumerate(pts):
            d.ellipse([q[0] - 6, q[1] - 6, q[0] + 6, q[1] + 6], fill=(255, 140, 0))
            d.text((q[0] + 8, q[1] - 6), str(i), fill=(255, 255, 255))
    return canvas.convert("RGB"), props, blocks, patrol


def main():
    out = next((a for a in sys.argv[1:] if not a.startswith("--")), "/tmp/town_preview.png")
    img, props, blocks, patrol = render("--blocks" in sys.argv)
    img.save(out)
    print(f"{len(props)} props, {len(blocks)} blocos de colisão -> {out}")
    problems = []
    for name, pt in [("spawn do jogador", (0, -3.9)), ("ervas oeste", (-5.5, -2)), ("ervas sudeste", (8, -6.5)),
                     ("ervas noroeste", (-10.2, 7.2)), ("poço (dica de água)", (0, -.3)),
                     ("estalagem (dica)", (-3, 4.25)), ("guilda (dica)", (4.3, 4.25))]:
        if not clear(pt[0], pt[1], blocks) and name not in ("poço (dica de água)", "estalagem (dica)", "guilda (dica)"):
            problems.append(f"{name} {pt} dentro de um bloco")
    for i, pt in enumerate(patrol):
        if not clear(pt[0], pt[1], blocks):
            problems.append(f"ponto de patrulha {i} {pt} dentro de um bloco")
    more, legs = check_patrol(patrol, blocks)
    problems += more
    print("patrulha: trechos concluídos =", legs)
    print("OK" if not problems else "\n".join("PROBLEMA: " + s for s in problems))


if __name__ == "__main__":
    main()
