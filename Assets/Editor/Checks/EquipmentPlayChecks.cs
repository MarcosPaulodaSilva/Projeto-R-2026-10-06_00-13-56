using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
namespace Vadronia.Editor
{
    public static class EquipmentPlayChecks
    {
        [MenuItem("Vadronia/Verificar equipamentos em Play")]
        public static void Run()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<VadroniaDemo>();
            if(!Application.isPlaying||game?.Equipment==null)throw new Exception("Entre em Play.");
            var state=game.Motor.State;var window=game.Equipment;
            string original=JsonUtility.ToJson(state.Progress);
            string path=Path.Combine(Application.persistentDataPath,"grunwald-adventure-v1.json");
            string[] paths={path,path+".previous",path+".tmp"};var backups=new byte[3][];
            for(int i=0;i<3;i++)if(File.Exists(paths[i]))backups[i]=File.ReadAllBytes(paths[i]);
            bool open=window.IsOpen;int count=0;
            Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception(name);count++;};
            try
            {
                state.Progress.inventory=InventoryRules.Starter();window.SetOpen(true);
                check(window.Root.Q("itemGrid").childCount==40,"40 itens visíveis");
                check(game.Sword==null,"Combate antigo desativado");
                using(var art=new EquipmentArt()){
                    foreach(var icon in art.Icons)check(icon!=null&&icon.rect.width>0,"Ícone importado");
                }
                for(int i=0;i<8;i++){
                    window.Select("sword-"+i);check(window.EquipSelected(),"Equipar espada");
                    check(InventoryRules.Equipped(state.Progress.inventory,GearSlot.Weapon)?.Id=="sword-"+i,"Espada equipada no inventário");
                }
                check(window.UnequipSelected(),"Retirar espada");
                check(game.Player.Transform.Find("Espada equipada")==null && game.Player.Transform.Find("Peitoral equipado")==null,"Personagem sem sobreposições");
                for(int i=0;i<4;i++){window.WearSet(i);check(InventoryRules.SetCount(state.Progress.inventory,ItemCatalog.Sets[i])==6,"Conjunto completo");}
                window.Select("sword-4");window.EquipSelected();
                var disk=JsonUtility.FromJson<AdventureProgress>(File.ReadAllText(path));
                check(InventoryRules.Equipped(disk.inventory,GearSlot.Weapon)?.Id=="sword-4","Espada salva");
                check(disk.coins==state.Progress.coins&&disk.quest==state.Progress.quest&&disk.herbs==state.Progress.herbs,"História preservada");
                window.Root.Q<TextField>("itemSearch").value="safira";
                check(window.Root.Q("itemGrid").childCount==1,"Busca por nome");
                window.Root.Q<TextField>("itemSearch").value="";
                window.ShowPage("loadout");check(!window.Root.Q("loadoutPage").ClassListContains("hidden"),"Aba equipamentos");
                window.ShowPage("status");check(!window.Root.Q("statusPage").ClassListContains("hidden"),"Aba status");
                check(state.MaxStamina==InventoryRules.Stats(state.Progress.inventory).Stamina,"Fôlego integrado");
                Debug.Log("EQUIPMENT PLAY PASS: "+count+" verificações.");
            }
            finally
            {
                state.Restore(JsonUtility.FromJson<AdventureProgress>(original));
                for(int i=0;i<3;i++){if(backups[i]!=null)File.WriteAllBytes(paths[i],backups[i]);else if(File.Exists(paths[i]))File.Delete(paths[i]);}
                window.ShowPage("inventory");window.SetOpen(open);
            }
        }
    }
}
