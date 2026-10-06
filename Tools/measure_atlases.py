"""Read-only image analysis: measures sprite rectangles/anchors and writes C# layout data.
Requires Pillow. Does not alter image pixels. Regions match the supplied generated atlases.
"""
from pathlib import Path
from PIL import Image
root = Path(__file__).resolve().parents[1]
parts = ['using UnityEngine;\nnamespace Vadronia\n{\n    internal static class WalkAtlasLayout\n    {']
for label, file, xs, ys in [
    ('Player', 'player-walk.png', [0,365,604,929,1254], [0,313,628,941,1254]),
    ('Conrad', 'conrad-walk.png', [0,363,603,941,1254], [0,319,627,939,1254])]:
    im = Image.open(root/'Assets/Resources/Vadronia'/file)
    alpha = im.getchannel('A').point(lambda a: 255 if a >= 50 else 0)
    rects, pivots, heights = [], [], []
    for row in range(4):
        for col in range(4):
            region = (xs[col], ys[row], xs[col+1], ys[row+1])
            b = alpha.crop(region).getbbox()
            assert b, (label,row,col)
            x,y = region[0]+b[0],region[1]+b[1]
            w,h = b[2]-b[0],b[3]-b[1]
            head = alpha.crop((x,y,x+w,y+int(h*.25))).getbbox()
            anchor = (head[0]+head[2])/2/w
            rects.append(f'            new Rect({x}, {im.height-y-h}, {w}, {h}),')
            pivots.append(f'            new Vector2({anchor:.6f}f, 0),')
            heights.append(h)
    parts.append(f'        internal const float {label}PPU = {sum(heights)/len(heights)/1.65:.6f}f;')
    parts.append(f'        internal static readonly Rect[] {label} =\n        {{\n'+ '\n'.join(rects)+'\n        };')
    parts.append(f'        internal static readonly Vector2[] {label}Pivots =\n        {{\n'+ '\n'.join(pivots)+'\n        };')
parts.append('    }\n}\n')
(root/'Assets/Scripts/Visual/WalkAtlasLayout.cs').write_text('\n'.join(parts))
im = Image.open(root/'Assets/Resources/Vadronia/town.png')
alpha = im.getchannel('A').point(lambda a: 255 if a >= 50 else 0)
regions = [(0,0,338,510),(340,0,778,510),(780,0,1174,510),(1175,0,1536,510),
           (0,530,420,1024),(425,530,775,1024),(795,640,1100,1024),(1120,600,1536,1024)]
rects=[]
for region in regions:
    b=alpha.crop(region).getbbox();assert b
    x,y=region[0]+b[0],region[1]+b[1];w,h=b[2]-b[0],b[3]-b[1]
    rects.append(f'            new Rect({x}, {im.height-y-h}, {w}, {h}),')
(root/'Assets/Scripts/Visual/TownAtlasLayout.cs').write_text('using UnityEngine;\nnamespace Vadronia\n{\n    internal static class TownAtlasLayout\n    {\n        internal static readonly Rect[] Frames =\n        {\n'+'\n'.join(rects)+'\n        };\n    }\n}\n')
print('Measured 32 walk frames and 8 scenery sprites; no image pixels changed.')
