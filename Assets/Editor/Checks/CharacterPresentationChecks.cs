using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Vadronia.Editor
{
    public static class CharacterPresentationChecks
    {
        [MenuItem("Vadronia/Verificar personagens e escalas em Play")]
        public static void Run()
        {
            if(!Application.isPlaying)throw new Exception("Entre em Play.");
            int count=0;
            Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception(message);count++;};
            using(var actor=new CharacterView("Teste apresentação",new FootPoint(0,-6),null,null,false))
            {
                var sr=actor.Transform.GetComponent<SpriteRenderer>();
                var dirs=new[]{Vector2.down,new Vector2(1,-1),Vector2.right,new Vector2(1,1),Vector2.up,new Vector2(-1,1),Vector2.left,new Vector2(-1,-1)};
                for(int d=0;d<8;d++)
                {
                    var seen=new HashSet<Sprite>();
                    for(int i=0;i<50;i++){
                        var pos=actor.Position;var step=dirs[d].normalized*.045f;
                        actor.Place(new FootPoint(pos.X+step.x,pos.Y+step.y));actor.Animate(1f/60,false,true);seen.Add(sr.sprite);
                    }
                    check(actor.Facing==d && actor.IsRunningAnimation && seen.Count>=15,"Corrida completa na direção "+d);
                    check(sr.flipX==(d==2||d==5||d==7),"Espelhamento explícito da corrida "+d);
                    actor.Place(actor.Position);actor.Animate(0);
                    check(!actor.IsRunningAnimation && sr.sprite.texture.name=="player-idle"&&!sr.flipX,"Idle próprio na direção "+d);
                    var idle=sr.sprite;float phase=actor.AnimationPhase;
                    for(int i=0;i<60;i++){actor.Place(actor.Position);actor.Animate(1f/60);}
                    check(sr.sprite==idle && actor.AnimationPhase==phase,"Repouso não desliza nem troca de quadro "+d);
                    check(sr.bounds.size.y>.95f&&sr.bounds.size.y<1.1f,"Altura coerente do protagonista "+d);
                }
            }
            foreach(string name in new[]{"Konrad","HELGA","BRUNO","MAREN","LÚCIA","TOMÁS","Moradora — feira oeste","Morador — feira leste"})
            using(var npc=new CharacterView(name,new FootPoint(0,-6),new NpcLook()))
            {
                check(npc.UsesReference,"Modelo fornecido aplicado a "+name);
                var sr=npc.Transform.GetComponent<SpriteRenderer>();
                foreach(var dir in new[]{Vector2.down,Vector2.right,Vector2.up,Vector2.left}){
                    npc.Face(dir);npc.Place(npc.Position);npc.Animate(0);
                    check(sr.bounds.size.y>.95f&&sr.bounds.size.y<1.1f,"Escala por direção de "+name);
                }
                float phase=npc.AnimationPhase;
                npc.Place(new FootPoint(.18f,-6));npc.Animate(.1f);
                check(Mathf.Abs(npc.AnimationPhase-phase-.25f)<.001f,"Passada proporcional ao deslocamento de "+name);
                check(npc.Transform.localScale==Vector3.one,"Sem squash artificial em "+name);
            }
            foreach(var prop in TownLayout.Props){
                if(!((prop.Atlas==0&&prop.Sprite<=3)||(prop.Atlas==1&&prop.Sprite<=6)))continue;
                var renderer=GameObject.Find(prop.Name).GetComponent<SpriteRenderer>();
                check(renderer.bounds.size.y>=3f&&renderer.bounds.size.y<=6f,"Construção proporcional: "+prop.Name);
            }
            Debug.Log(count+" verificações de personagens e escalas em Play passaram.");
        }
    }
}
