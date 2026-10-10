using System;
using System.Collections.Generic;
namespace Vadronia
{
    public enum GearSlot { Weapon, Head, Chest, Hands, Legs, Feet, Cloak, Accessory }
    [Serializable] public sealed class ItemStack { public string id; public int quantity=1; public ItemStack(string id){this.id=id;} }
    [Serializable] public sealed class EquippedItem { public int slot; public string id; public EquippedItem(int slot,string id){this.slot=slot;this.id=id;} }
    [Serializable] public sealed class InventoryData
    {
        public int version=1;
        public List<ItemStack> items=new List<ItemStack>();
        public List<EquippedItem> equipped=new List<EquippedItem>();
    }
    public struct GearStats
    {
        public int Vitality,Strength,Dexterity,Intelligence,Vigor,Defense,Attack;
        public float Weight;
        public int Health=>50+Vitality*5;
        public int Stamina=>50+Vigor*5;
        public int Physical=>Strength*2+Attack;
    }
    public sealed class GearItem
    {
        public readonly string Id,Name,Description,Set;
        public readonly GearSlot Slot;
        public readonly int Icon,Rarity,Style;
        public readonly GearStats Bonus;
        public GearItem(string id,string name,GearSlot slot,int icon,int style,int rarity,string set,string description,GearStats bonus)
        {Id=id;Name=name;Slot=slot;Icon=icon;Style=style;Rarity=rarity;Set=set;Description=description;Bonus=bonus;}
    }
    public static class ItemCatalog
    {
        public static readonly string[] Slots={"Espada","Cabeça","Peitoral","Luvas","Calças","Botas","Capa","Acessório"};
        public static readonly string[] Sets={"Viajante","Batedor","Guarda","Guilda"};
        public static readonly string[] Rarities={"Comum","Refinado","Raro"};
        public static readonly GearItem[] All=Build();
        public static GearItem Find(string id){if(id==null)return null;foreach(var item in All)if(item.Id==id)return item;return null;}
        static GearItem[] Build()
        {
            var list=new List<GearItem>();
            string[] swords={"Espada de ferro","Espada de aço","Florete dourado","Falcata do batedor","Espada da guarda","Lâmina do inverno","Espada da guilda","Montante de aço"};
            for(int i=0;i<8;i++)list.Add(new GearItem("sword-"+i,swords[i],GearSlot.Weapon,i,i,i/3,"","Lâmina equipável no inventário. Altera atributos e aparece na mão esquerda. A roupa do personagem permanece original. Combate em uma próxima etapa.",new GearStats{Attack=5+i*2,Strength=i==7?3:0,Dexterity=i==2?3:0,Weight=1.2f+i*.2f}));
            string[,] names={{"Barrete de viagem","Túnica de viagem","Luvas de viagem","Calças de viagem","Botas de viagem","Capa de viagem"},{"Capuz do batedor","Gibão do batedor","Luvas do batedor","Calças do batedor","Botas do batedor","Capa do batedor"},{"Elmo da guarda","Couraça da guarda","Manoplas da guarda","Calças da guarda","Botas da guarda","Capa da guarda"},{"Diadema da guilda","Casaco da guilda","Braçadeiras da guilda","Calças da guilda","Botas da guilda","Manto da guilda"}};
            for(int set=0;set<4;set++)for(int slot=0;slot<6;slot++)
                list.Add(new GearItem("gear-"+set+"-"+slot,names[set,slot],(GearSlot)(slot+1),8+set*6+slot,set,Math.Min(set,2),Sets[set],"Peça do conjunto "+Sets[set]+". Combine livremente com outros conjuntos. 3 peças: +2 vigor; 6 peças: +5 defesa.",new GearStats{Defense=1+set+(slot==1?2:0),Vitality=slot==1?2:0,Dexterity=set==1?1:0,Intelligence=set==3?1:0,Vigor=slot==4?1:0,Weight=.3f+set*.25f+(slot==1?.7f:0)}));
            string[] accessories={"Anel de cobre","Anel de safira","Anel de âmbar","Pingente de cristal","Talismã de couro","Broche da folha","Medalhão da guilda","Amuleto de ferro"};
            for(int i=0;i<8;i++)list.Add(new GearItem("accessory-"+i,accessories[i],GearSlot.Accessory,32+i,i,i/3,"","Acessório único equipado. Os bônus aparecem imediatamente nos status.",new GearStats{Vitality=i%4==0?3:0,Strength=i%4==1?2:0,Dexterity=i%4==2?3:0,Intelligence=i%4==3?3:0,Vigor=i>=4?2:0,Weight=.1f}));
            return list.ToArray();
        }
    }
    public static class InventoryRules
    {
        public static InventoryData Starter()
        {
            var data=new InventoryData();foreach(var item in ItemCatalog.All)data.items.Add(new ItemStack(item.Id));
            Equip(data,"sword-0");for(int i=0;i<6;i++)Equip(data,"gear-0-"+i);Equip(data,"accessory-0");return data;
        }
        public static bool Owns(InventoryData data,string id)=>data!=null&&data.items!=null&&data.items.Exists(x=>x!=null&&x.id==id&&x.quantity>0);
        public static GearItem Equipped(InventoryData data,GearSlot slot)
        {if(data?.equipped==null)return null;foreach(var e in data.equipped)if(e!=null&&e.slot==(int)slot&&Owns(data,e.id)){var item=ItemCatalog.Find(e.id);if(item!=null&&item.Slot==slot)return item;}return null;}
        public static bool Equip(InventoryData data,string id)
        {
            var item=ItemCatalog.Find(id);if(item==null||!Owns(data,id))return false;
            if(data.equipped==null)data.equipped=new List<EquippedItem>();
            data.equipped.RemoveAll(e=>e==null||e.slot==(int)item.Slot||e.id==id);
            data.equipped.Add(new EquippedItem((int)item.Slot,id));return true;
        }
        public static bool Unequip(InventoryData data,GearSlot slot)=>data?.equipped!=null&&data.equipped.RemoveAll(e=>e!=null&&e.slot==(int)slot)>0;
        public static InventoryData Clean(InventoryData source)
        {
            if(source==null)return Starter();
            var clean=new InventoryData();var seen=new HashSet<string>();
            if(source.items!=null)foreach(var stack in source.items)
                if(stack!=null&&stack.quantity>0&&ItemCatalog.Find(stack.id)!=null&&seen.Add(stack.id)&&clean.items.Count<64)clean.items.Add(new ItemStack(stack.id));
            var slots=new HashSet<int>();
            if(source.equipped!=null)foreach(var entry in source.equipped){
                if(entry==null)continue;var item=ItemCatalog.Find(entry.id);
                if(item!=null&&(int)item.Slot==entry.slot&&Owns(clean,entry.id)&&slots.Add(entry.slot))Equip(clean,entry.id);
            }
            return clean;
        }
        public static int SetCount(InventoryData data,string set){int n=0;for(int i=0;i<8;i++)if(Equipped(data,(GearSlot)i)?.Set==set)n++;return n;}
        public static GearStats Stats(InventoryData data,string compare=null)
        {
            var stats=new GearStats{Vitality=10,Strength=5,Dexterity=5,Intelligence=5,Vigor=10};
            var candidate=ItemCatalog.Find(compare);int[] sets=new int[4];
            for(int i=0;i<8;i++){
                var item=candidate!=null&&(int)candidate.Slot==i?candidate:Equipped(data,(GearSlot)i);if(item==null)continue;var b=item.Bonus;
                stats.Vitality+=b.Vitality;stats.Strength+=b.Strength;stats.Dexterity+=b.Dexterity;stats.Intelligence+=b.Intelligence;stats.Vigor+=b.Vigor;stats.Defense+=b.Defense;stats.Attack+=b.Attack;stats.Weight+=b.Weight;
                if(item.Slot>=GearSlot.Head&&item.Slot<=GearSlot.Cloak)sets[item.Style]++;
            }
            foreach(int count in sets){if(count>=3)stats.Vigor+=2;if(count>=6)stats.Defense+=5;}
            return stats;
        }
    }
}
