using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
namespace Vadronia.Editor
{
    public static class AdventurePlayChecks
    {
        [MenuItem("Vadronia/Verificar exploração em Play")]
        public static void Run()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<VadroniaDemo>();
            if(!Application.isPlaying||game==null||game.Motor==null)throw new Exception("Entre em Play antes desta verificação.");
            var motor=game.Motor;var actor=game.Player;var origin=actor.Position;
            string progress=JsonUtility.ToJson(motor.State.Progress);
            string path=Path.Combine(Application.persistentDataPath,"grunwald-adventure-v1.json");
            string[] paths={path,path+".previous",path+".tmp"};var backups=new byte[3][];
            for(int i=0;i<3;i++)if(File.Exists(paths[i]))backups[i]=File.ReadAllBytes(paths[i]);
            bool enabled=game.enabled;game.enabled=false;int count=0;
            Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception(message);Debug.Log("PLAY PASS: "+message);count++;};
            try
            {
                actor.Place(new FootPoint(0,-6));motor.Halt();
                for(int i=0;i<60;i++)motor.Tick(1f/60,Vector2.right,false,false);
                float walk=actor.Position.X;
                check(walk>2.2f&&walk<2.5f,"Caminhada percorre a distância esperada");
                actor.Place(new FootPoint(0,-6));motor.Halt();
                for(int i=0;i<60;i++)motor.Tick(1f/60,Vector2.right,true,false);
                check(actor.Position.X>walk*1.7f&&motor.State.Stamina<90,"Corrida aumenta velocidade e gasta fôlego");
                actor.Place(new FootPoint(0,-.3f));motor.Halt();motor.State.Rest();
                for(int i=0;i<30;i++)motor.Tick(1f/60,Vector2.up,false,i==0);
                check(actor.Position.Y<=-.219f&&MovementCore.Clear(actor.Position.X,actor.Position.Y,TownLayout.Blocks()),"Esquiva não atravessa o poço");
                for(int i=0;i<120;i++)motor.Tick(1f/60,Vector2.zero,false,false);
                int restFrame=actor.AnimationFrame;float restPhase=actor.AnimationPhase;
                for(int i=0;i<30;i++)motor.Tick(1f/60,Vector2.zero,false,false);
                check(!actor.Cycle.Moving&&actor.AnimationFrame==restFrame&&actor.AnimationPhase==restPhase,"Parada mantém pose e fase sem caminhar no lugar");
                game.Hud.SetPause(true);check(game.Hud.Paused,"Pausa é ativada");game.Hud.SetPause(false);
                motor.State.Restore(new AdventureProgress());game.Interactions.RefreshPlants();
                game.Interactions.Interact(0);check(game.Hud.DialogOpen&&motor.State.Progress.quest==1,"Conversa com Konrad inicia missão e diálogo");game.Hud.CloseDialog();
                for(int i=2;i<=4;i++)game.Interactions.Interact(i);
                check(motor.State.HerbCount==3,"Três canteiros coletados nas interações");
                game.Interactions.Interact(0);game.Hud.CloseDialog();game.Interactions.Interact(0);game.Hud.CloseDialog();
                check(motor.State.Progress.quest==2&&motor.State.Progress.coins==25,"Entrega da missão concede 25 moedas uma única vez");
                var loaded=JsonUtility.FromJson<AdventureProgress>(File.ReadAllText(path));
                check(loaded.quest==2&&loaded.herbs==7&&loaded.coins==25,"Save em disco conserva missão, ervas e recompensa");
                check(Vozes.Active(motor.State),"Konrad oferece Vozes da vila depois das ervas");
                game.Interactions.Interact(7);game.Hud.CloseDialog();
                game.Interactions.Interact(7);game.Hud.CloseDialog();
                game.Interactions.Interact(8);game.Hud.CloseDialog();
                check(Vozes.Heard(motor.State)==2&&motor.State.Progress.coins==25,"Mural e banca registram dois relatos sem duplicar leituras");
                game.Interactions.Interact(0);game.Hud.CloseDialog();
                check(!motor.State.HasFlag(StoryFlags.VozesConcluida),"Dois relatos ainda não concluem a missão");
                game.Interactions.Interact(6);game.Hud.CloseDialog();
                game.Interactions.Interact(0);game.Hud.CloseDialog();
                check(motor.State.Progress.coins==40&&motor.State.HasFlag(StoryFlags.VozesConcluida),"Três relatos e retorno a Konrad pagam 15 moedas");
                motor.State.Restore(JsonUtility.FromJson<AdventureProgress>(File.ReadAllText(path)));
                game.Interactions.Interact(0);game.Hud.CloseDialog();
                check(motor.State.Progress.coins==40&&Vozes.Heard(motor.State)==3,"Save real recarrega relatos e não paga a missão novamente");
                motor.State.Restore(JsonUtility.FromJson<AdventureProgress>("{\"version\":1,\"quest\":2,\"herbs\":7,\"coins\":25}"));
                check(motor.State.Progress.flags!=null&&motor.State.Progress.flags.Count==0&&motor.State.Progress.coins==25,"JSON antigo sem flags preserva o progresso");
                foreach(int point in new[]{6,7,8}){game.Interactions.Interact(point);game.Hud.CloseDialog();}
                check(Vozes.Heard(motor.State)==3&&!Vozes.Active(motor.State),"Relatos podem ser ouvidos antes da oferta");
                game.Interactions.Interact(0);game.Hud.CloseDialog();
                check(Vozes.Active(motor.State)&&motor.State.Progress.coins==25,"Oferta reconhece relatos anteriores sem pular diálogo");
                game.Interactions.Interact(0);game.Hud.CloseDialog();
                check(motor.State.Progress.coins==40,"Conclusão com relatos anteriores paga uma única recompensa");
                check(game.Interactions.Npcs!=null&&game.Interactions.Npcs.Count==NpcRoster.All.Length,"Integração conserva os cinco NPCs da main");
                for(int i=0;i<NpcRoster.All.Length;i++)
                {
                    game.Interactions.Interact(100+i);
                    check(game.Hud.DialogOpen,"Conversa preservada com "+NpcRoster.All[i].Name);
                    game.Hud.CloseDialog();
                }
                var document=UnityEngine.Object.FindFirstObjectByType<UIDocument>();
                check(document.rootVisualElement.Q<Label>("questText")!=null&&document.panelSettings.textSettings.defaultFontAsset!=null,"Interface tem objetivo e fonte configurados");
                var directions=new[]{Vector2.down,new Vector2(1,-1),Vector2.right,new Vector2(1,1),Vector2.up,new Vector2(-1,1),Vector2.left,new Vector2(-1,-1)};
                string[] textures={"south","southeast","east","northeast","north","northwest","west","southeast"};
                for(int d=0;d<8;d++)
                {
                    actor.Place(new FootPoint(0,-6));motor.Halt();
                    var seen=new System.Collections.Generic.HashSet<int>();
                    for(int i=0;i<45;i++){motor.Tick(1f/60,directions[d],false,false);seen.Add(actor.AnimationFrame);}
                    var sprite=actor.Transform.GetComponent<SpriteRenderer>();
                    check(actor.UsesVideo&&actor.DirectionCount==8&&actor.Facing==d&&sprite.sprite.texture.name==textures[d]
                        &&sprite.flipX==(d==7)&&seen.Count>=15,"Direção "+d+" usa o vídeo e avança quadros completos");
                }
                var npc=game.Conrad;var npcRenderer=npc.Transform.GetComponent<SpriteRenderer>();
                bool originalNpc=!npc.UsesVideo&&npc.DirectionCount==4;
                foreach(var d in new[]{Vector2.down,Vector2.right,Vector2.up,Vector2.left})
                {npc.Face(d);npc.Animate(0);originalNpc&=npcRenderer.sprite.texture==game.characterAtlas;}
                check(originalNpc,"Konrad conserva o atlas original nas quatro direções");
                actor.Place(new FootPoint(0,-.221f));motor.Halt();
                for(int i=0;i<60;i++)motor.Tick(1f/60,Vector2.up,false,false);
                float blockedPhase=actor.AnimationPhase;
                for(int i=0;i<60;i++)motor.Tick(1f/60,Vector2.up,false,false);
                check(!actor.Cycle.Moving&&Mathf.Abs(actor.AnimationPhase-blockedPhase)<.00001f,"Colisão interrompe avanço da animação mesmo mantendo a direção pressionada");
                actor.Place(new FootPoint(0,-.3f));motor.Halt();motor.Lunge(Vector2.up,2);
                check(actor.Position.Y<=-.219f&&MovementCore.Clear(actor.Position.X,actor.Position.Y,TownLayout.Blocks()),"Investida da espada respeita colisão do poço");
                Debug.Log(count+" verificações de exploração em Play passaram; progresso anterior será restaurado.");
            }
            finally
            {
                for(int i=0;i<paths.Length;i++)
                    if(backups[i]!=null)File.WriteAllBytes(paths[i],backups[i]);else if(File.Exists(paths[i]))File.Delete(paths[i]);
                motor.State.Restore(JsonUtility.FromJson<AdventureProgress>(progress));motor.State.Rest();actor.Place(origin);motor.Halt();
                game.Interactions.RefreshPlants();game.Hud.CloseDialog();game.Hud.SetPause(false);game.enabled=enabled;
            }
        }
    }
}
