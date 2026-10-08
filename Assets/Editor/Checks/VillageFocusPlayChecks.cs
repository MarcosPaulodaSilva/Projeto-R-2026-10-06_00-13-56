using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Vadronia.Editor
{
    public static class VillageFocusPlayChecks
    {
        [MenuItem("Vadronia/Verificar vila e interações em Play")]
        public static void Run()
        {
            var game=UnityEngine.Object.FindAnyObjectByType<VadroniaDemo>();
            if(!Application.isPlaying||game?.Motor==null)throw new Exception("Entre em Play antes de verificar a vila.");
            bool enabled=game.enabled;game.enabled=false;
            var pos=game.Player.Position;var facing=game.Motor.Heading;var npcs=game.Interactions.Npcs;
            var conradPosition=game.Conrad.Position;
            game.Interactions.Npcs=null;int count=0;
            Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception(message);count++;Debug.Log("VILLAGE PASS: "+message);};
            Action<float,float,Vector2> stand=(x,y,direction)=>
            {game.Player.Place(new FootPoint(x,y));game.Motor.Halt();game.Player.Face(direction);game.Interactions.SelectTarget();};
            try
            {
                stand(5.4f,2.05f,Vector2.up);
                check(game.Interactions.Target==7&&game.Interactions.Hint.Contains("mural"),"Frente do mural seleciona o mural real");
                stand(6.2f,2.57f,Vector2.left);
                check(game.Interactions.Target!=7,"Lateral do mural não aciona leitura");
                stand(5.4f,1.55f,Vector2.up);
                check(game.Interactions.Target!=7,"Mural fora do alcance não recebe T");
                game.Conrad.Place(new FootPoint(4.98f,2.22f));
                stand(5.4f,2.12f,Vector2.up);
                check(game.Interactions.Target==7,"Mural à frente vence NPC próximo ao lado");
                stand(5.4f,2.12f,Vector2.left);
                check(game.Interactions.Target==0,"Virar para Konrad troca o alvo sem prioridade fixa");
                game.Conrad.Place(conradPosition);
                foreach(var b in VillageBuildings.All)
                {
                    stand(b.Door.X,b.Door.Y-.24f,Vector2.up);
                    check(game.Interactions.Target==b.Choice,"Porta identifica "+b.Name);
                }
                var panel=UnityEngine.Object.FindAnyObjectByType<UIDocument>();
                check(panel.rootVisualElement.Q<Label>("buildingIdentity").text.Contains("Casa do bosque"),"Identificação do prédio aparece na interface");
                game.Interactions.Interact(20);
                check(game.Hud.DialogOpen&&panel.rootVisualElement.Q<Label>("speaker").text.Contains("PREFEITURA"),"Prefeitura abre o comunicado correto");game.Hud.CloseDialog();
                game.Interactions.Interact(34);
                check(game.Hud.DialogOpen,"Casa de morador tem identificação própria");game.Hud.CloseDialog();
                var renderers=UnityEngine.Object.FindObjectsByType<SpriteRenderer>();
                bool hall=false,guild=false,board=false,ground=false;
                foreach(var r in renderers)
                {
                    if(r.name=="Prefeitura — bandeira azul")hall=r.sprite.texture.name=="town-blue";
                    if(r.name=="Guilda de Grünwald")guild=r.sprite.texture.name=="town-blue";
                    if(r.name=="Mural de avisos")board=r.sprite.texture.name=="town-blue";
                    if(r.name=="Chão")ground=r.sprite.texture.name=="terrain-blue";
                }
                check(hall&&guild&&board&&ground,"Cena usa os novos sprites e o terreno das referências");
                Debug.Log(count+" verificações da vila em Play passaram; nenhum save foi alterado.");
            }
            finally
            {
                game.Interactions.Npcs=npcs;game.Player.Place(pos);game.Motor.Halt();game.Player.Face(facing);
                game.Conrad.Place(conradPosition);
                game.Hud.CloseDialog();game.Interactions.SelectTarget();game.enabled=enabled;
            }
        }
    }
}
