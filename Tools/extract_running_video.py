"""Extract supplied running video; requires pillow, numpy and imageio-ffmpeg."""
from pathlib import Path
import subprocess, hashlib, json
import numpy as np
from PIL import Image,ImageDraw
import imageio_ffmpeg
ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'docs/references/gemini_generated_video_b0983924.mp4'
OUT=ROOT/'Assets/Resources/Vadronia/player-run'
OUT.mkdir(parents=True,exist_ok=True)
raw=subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-v','error','-i',str(SOURCE),'-vf','crop=400:600:440:60,scale=200:300:flags=area','-f','rawvideo','-pix_fmt','rgb24','-'],capture_output=True,check=True).stdout
video=np.frombuffer(raw,np.uint8).reshape(-1,300,200,3)
# Stable segments between hard cuts. All unrepresented left/right views are explicit mirrors.
clips=[('south',1,23),('southeast',150,22),('east',62,22),('northeast',205,17),('north',31,23),('northwest',205,17),('west',62,22),('southwest',150,22)]
manifest=dict(source='docs/references/'+SOURCE.name,sourceSha256=hashlib.sha256(SOURCE.read_bytes()).hexdigest(),sourceFps=24,frameWidth=200,frameHeight=300,columns=8,pivotX=.5,pivotY=.05,pixelsPerUnit=260,clips=[])
for name,start,count in clips:
    sheet=Image.new('RGBA',(1600,900))
    for i in range(count):
        rgb=video[start+i]; a=rgb.astype(np.int16)
        gray=(a.max(2)-a.min(2)<12)&(a.max(2)<80)
        mask=Image.fromarray(np.where(gray,0,255).astype('uint8')).copy()
        ImageDraw.floodfill(mask,(0,0),128,thresh=0)
        fg=Image.fromarray(np.where(np.asarray(mask)==128,0,255).astype('uint8')).copy()
        ImageDraw.floodfill(fg,(100,140),128,thresh=0)
        alpha=np.where(np.asarray(fg)==128,255,0).astype('uint8')
        assert np.count_nonzero(alpha)>4000, (name,i, np.count_nonzero(alpha),rgb[140,100].tolist())
        im=Image.fromarray(np.dstack((rgb,alpha)))
        sheet.paste(im,(i%8*200,i//8*300))
    sheet.save(OUT/(name+'.png'))
    manifest['clips'].append(dict(name=name,sourceStartFrame=start,frameCount=count,idleFrame=0,mirror=name in ['east','northwest','southwest']))
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('Exported',len(clips),'running clips from supplied video')



