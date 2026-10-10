using System;
using System.Collections.Generic;
namespace Vadronia
{
 public static class InventoryChecks
 {
  public static int Run(Action<string> log)
  {
   int n=0;Action<bool,string> check=(ok,text)=>{if(!ok)throw new Exception(text);n++;log("PASS: "+text);};
   var data=InventoryRules.Starter();var ids=new HashSet<string>();
   check(ItemCatalog.All.Length==40,"Catálogo contém 40 itens");
   foreach(var item in ItemCatalog.All){check(ids.Add(item.Id),"ID exclusivo: "+item.Id);check(InventoryRules.Equip(data,item.Id)&&InventoryRules.Equipped(data,item.Slot)==item,"Item equipa no espaço correto: "+item.Name);}
   check(data.items.Count==40&&data.equipped.Count==8,"Trocas não duplicam nem consomem os itens");
   check(!InventoryRules.Equip(data,"inexistente"),"Item desconhecido rejeitado");
   var empty=new InventoryData();check(!InventoryRules.Equip(empty,"sword-0"),"Item não possuído rejeitado");
   check(InventoryRules.Clean(empty).items.Count==0,"Inventário vazio salvo não recebe kit novamente");
   check(InventoryRules.Clean(null).items.Count==40,"Save antigo recebe kit inicial uma vez");
   data.items.Add(new ItemStack("sword-0"));data.items.Add(new ItemStack("inexistente"));data.items.Add(new ItemStack("sword-1"){quantity=-9});
   data.equipped.Add(new EquippedItem(1,"sword-0"));data.equipped.Add(new EquippedItem(99,"sword-0"));
   data=InventoryRules.Clean(data);check(data.items.Count==40&&data.equipped.Count==8,"Dados duplicados, inválidos e quantidades negativas são saneados");
   for(int i=0;i<8;i++)InventoryRules.Unequip(data,(GearSlot)i);
   var bare=InventoryRules.Stats(data);check(bare.Health==100&&bare.Stamina==100&&bare.Defense==0,"Retirar tudo volta aos atributos base");
   for(int i=0;i<3;i++)InventoryRules.Equip(data,"gear-2-"+i);
   check(InventoryRules.Stats(data).Vigor==12,"Três peças aplicam bônus de vigor uma vez");
   for(int i=3;i<6;i++)InventoryRules.Equip(data,"gear-2-"+i);
   int defense=0;for(int i=0;i<6;i++)defense+=ItemCatalog.Find("gear-2-"+i).Bonus.Defense;
   check(InventoryRules.Stats(data).Defense==defense+5,"Seis peças aplicam bônus de defesa");
   var prediction=InventoryRules.Stats(data,"sword-7");InventoryRules.Equip(data,"sword-7");
   check(prediction.Physical==InventoryRules.Stats(data).Physical,"Comparação corresponde ao resultado equipado");
   for(int i=0;i<1000;i++){var item=ItemCatalog.All[i%40];InventoryRules.Equip(data,item.Id);if(i%3==0)InventoryRules.Unequip(data,item.Slot);var stats=InventoryRules.Stats(data);if(stats.Health<100||stats.Stamina<100||float.IsNaN(stats.Weight)||data.items.Count!=40)throw new Exception("Estado corrompido em trocas repetidas");}
   check(true,"Mil trocas preservam posse e atributos válidos");
   return n;
  }
 }
}
