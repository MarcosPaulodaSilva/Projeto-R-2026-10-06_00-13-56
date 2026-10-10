using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Vadronia.Editor
{
    public static class SwordCycleChecks
    {
        [MenuItem("Vadronia/Verificar ciclos completos da espada")]
        public static void Run()
        {
            if(!Application.isPlaying)throw new Exception("Entre em Play.");
            int assertions=0;
            Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);assertions++;};
            using(var art=new EquipmentArt())
            using(var actor=new CharacterView("Verificação de ciclos",new FootPoint(0,0),null,null,false))
            using(var sword=new HeldSword(actor,art))
            {
                foreach(var t in actor.Transform.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
                var inventory=InventoryRules.Starter();
                var body=actor.Transform.GetComponent<SpriteRenderer>();
                for(int d=0;d<8;d++){
                    float angle=d*Mathf.PI/4;var direction=new Vector2(Mathf.Sin(angle),-Mathf.Cos(angle));
                    for(int mode=0;mode<2;mode++){
                        var visited=new HashSet<int>();
                        float step=(mode==1?1.65f:1.25f)/128f;
                        for(int i=0;i<256;i++){
                            var p=actor.Position;
                            actor.Place(new FootPoint(p.X+direction.x*step,p.Y+direction.y*step));
                            actor.Animate(1f/60,false,mode==1);sword.Apply(inventory);
                            visited.Add(actor.AnimationFrame);
                            check(sword.Renderer.enabled&&sword.ItemId=="sword-0","Arma permanece equipada");
                            check(actor.Facing==d&&actor.AnimationFrame<actor.FrameCount,"Direção e quadro válidos");
                            check(sword.HandPosition.y>.2f&&sword.HandPosition.y<.8f&&Mathf.Abs(sword.HandPosition.x)<.4f,"Empunhadura permanece no braço");
                        }
                        check(visited.Count==actor.FrameCount,"Todos os quadros do ciclo foram percorridos");
                        actor.Place(actor.Position);actor.Animate(0);sword.Apply(inventory);
                        var stopped=sword.HandPosition;var pose=body.sprite;
                        for(int i=0;i<20;i++){actor.Animate(.1f);sword.Apply(inventory);}
                        check(stopped==sword.HandPosition&&pose==body.sprite&&!actor.IsRunningAnimation,"Parada sem deriva");
                    }
                }
                var original=body.sprite;body.sprite=null;sword.Apply(inventory);
                check(!sword.Renderer.enabled,"Sem corpo não há arma flutuante");
                body.sprite=original;sword.Apply(inventory);
                check(sword.Renderer.enabled,"Arma retorna com o corpo");
                InventoryRules.Unequip(inventory,GearSlot.Weapon);sword.Apply(inventory);
                check(!sword.Renderer.enabled,"Retirar arma esconde apresentação");
            }
            Debug.Log("SWORD CYCLES PASS: "+assertions+" verificações em ciclos completos, transições e parada.");
        }
    }
}
