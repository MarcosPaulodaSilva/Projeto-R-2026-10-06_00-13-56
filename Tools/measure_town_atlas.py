"""Measure the active town atlas and regenerate TownAtlasLayout.cs.

Read-only for image pixels. Requires Pillow.
"""
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[1]
image = Image.open(root / "Assets/Resources/Vadronia/town.png")
alpha = image.getchannel("A").point(lambda value: 255 if value >= 50 else 0)

regions = [
    (0, 0, 338, 510),
    (340, 0, 778, 510),
    (780, 0, 1174, 510),
    (1175, 0, 1536, 510),
    (0, 530, 420, 1024),
    (425, 530, 775, 1024),
    (795, 640, 1100, 1024),
    (1120, 600, 1536, 1024),
]

rects = []
for region in regions:
    bounds = alpha.crop(region).getbbox()
    assert bounds, region
    x = region[0] + bounds[0]
    y = region[1] + bounds[1]
    width = bounds[2] - bounds[0]
    height = bounds[3] - bounds[1]
    rects.append(
        f"            new Rect({x}, {image.height-y-height}, {width}, {height}),"
    )

output = (
    "using UnityEngine;\n"
    "namespace Vadronia\n"
    "{\n"
    "    internal static class TownAtlasLayout\n"
    "    {\n"
    "        internal static readonly Rect[] Frames =\n"
    "        {\n"
    + "\n".join(rects)
    + "\n        };\n"
    "    }\n"
    "}\n"
)

(root / "Assets/Scripts/Visual/TownAtlasLayout.cs").write_text(output)
print("Measured 8 active scenery sprites; no image pixels changed.")
