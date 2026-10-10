using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Vadronia
{
    public sealed class EquipmentWindow:IDisposable
    {
        readonly AdventureState state;readonly Func<bool> save;readonly EquipmentArt art;
        readonly VisualElement root,grid;readonly TextField search;readonly DropdownField filter,sets;
        readonly HeldSword heldSword;readonly CharacterView preview;readonly Camera camera;readonly RenderTexture target;
        string selected="sword-0",page="inventory";int facing;
        public bool IsOpen {get;private set;}
        public string Selected=>selected;
        public VisualElement Root=>root;
        public EquipmentWindow(VisualElement parent,AdventureState state,EquipmentArt art,Func<bool> save)
        {
            this.state=state;this.art=art;this.save=save;
            var tree=Resources.Load<VisualTreeAsset>("Vadronia/UI/Equipment");tree.CloneTree(parent);root=parent.Q("equipmentWindow");grid=root.Q("itemGrid");
            search=root.Q<TextField>("itemSearch");filter=root.Q<DropdownField>("slotFilter");sets=root.Q<DropdownField>("setPicker");
            filter.choices=new List<string>{"Todos"};filter.choices.AddRange(ItemCatalog.Slots);filter.index=0;
            sets.choices=new List<string>(ItemCatalog.Sets);sets.index=0;
            search.RegisterValueChangedCallback(_=>RefreshGrid());filter.RegisterValueChangedCallback(_=>RefreshGrid());
            root.Q<Button>("closeEquipment").clicked+=()=>SetOpen(false);
            root.Q<Button>("inventoryTab").clicked+=()=>ShowPage("inventory");root.Q<Button>("loadoutTab").clicked+=()=>ShowPage("loadout");root.Q<Button>("statusTab").clicked+=()=>ShowPage("status");
            root.Q<Button>("equipSelected").clicked+=()=>EquipSelected();root.Q<Button>("unequipSelected").clicked+=()=>UnequipSelected();
            root.Q<Button>("rotateLeft").clicked+=()=>Rotate(-1);root.Q<Button>("rotateRight").clicked+=()=>Rotate(1);
            root.Q<Button>("wearSet").clicked+=()=>WearSet(sets.index);
            preview=new CharacterView("Prévia de equipamento",new FootPoint(1000,1000),null,null,false);
            heldSword=new HeldSword(preview,art);
            foreach(var t in preview.Transform.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
            camera=new GameObject("Câmera do equipamento").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.76f;camera.aspect=.8f;
            camera.transform.position=new Vector3(1000,1000.52f,-10);camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.13f,.19f);camera.allowHDR=false;
            target=new RenderTexture(384,480,24);camera.targetTexture=target;root.Q<Image>("characterPreview").image=target;
            Refresh();SetOpen(false);
        }
        public void SetOpen(bool open){IsOpen=open;root.EnableInClassList("hidden",!open);if(open){Refresh();root.Q<Label>("equipmentMessage").text="";}}
        public void HandleInput(bool allowed)
        {
            if(ExplorerMotor.Pressed(KeyCode.Escape)&&IsOpen){SetOpen(false);return;}
            if(IsOpen && root.panel?.focusController?.focusedElement is VisualElement focused && (focused==search||search.Contains(focused)))return;
            if(ExplorerMotor.Pressed(KeyCode.I)&&allowed)SetOpen(!IsOpen);
        }
        public void Select(string id){if(ItemCatalog.Find(id)==null||!InventoryRules.Owns(state.Progress.inventory,id))return;selected=id;RefreshDetails();RefreshGrid();}
        public bool EquipSelected(){bool ok=InventoryRules.Equip(state.Progress.inventory,selected);if(ok)Changed("Equipado: "+ItemCatalog.Find(selected).Name);return ok;}
        public bool UnequipSelected(){var item=ItemCatalog.Find(selected);if(item==null||InventoryRules.Equipped(state.Progress.inventory,item.Slot)?.Id!=selected)return false;bool ok=InventoryRules.Unequip(state.Progress.inventory,item.Slot);if(ok)Changed("Equipamento retirado. O item continua no inventário.");return ok;}
        public void WearSet(int index)
        {
            if(index<0||index>3)return;
            for(int i=0;i<6;i++)if(!InventoryRules.Owns(state.Progress.inventory,"gear-"+index+"-"+i)){root.Q<Label>("equipmentMessage").text="Faltam peças desse conjunto.";return;}
            for(int i=0;i<6;i++)InventoryRules.Equip(state.Progress.inventory,"gear-"+index+"-"+i);
            Changed("Conjunto "+ItemCatalog.Sets[index]+" equipado. Espada e acessório mantidos.");
        }
        void Changed(string text){state.ClampStamina();bool saved=save==null||save();Refresh();root.Q<Label>("equipmentMessage").text=text+(saved?"":" Não foi possível salvar; tente F5.");}
        public void ShowPage(string value){page=value;foreach(string p in new[]{"inventory","loadout","status"}){root.Q(p+"Page").EnableInClassList("hidden",p!=page);root.Q<Button>(p+"Tab").EnableInClassList("selected",p==page);}}
        void Rotate(int step){facing=(facing+step+8)%8;RenderPreview();}
        void RenderPreview(){float angle=facing*Mathf.PI/4;preview.Face(new Vector2(Mathf.Sin(angle),-Mathf.Cos(angle)));preview.Place(preview.Position);preview.Animate(0);heldSword.Apply(state.Progress.inventory);camera.Render();}
        void Refresh()
        {
            if(!InventoryRules.Owns(state.Progress.inventory,selected))selected=state.Progress.inventory.items.Count>0?state.Progress.inventory.items[0].id:null;
            RefreshGrid();RefreshDetails();RefreshSlots();RefreshStats();ShowPage(page);RenderPreview();
            root.Q<Label>("equippedSummary").text="Espada: "+(InventoryRules.Equipped(state.Progress.inventory,GearSlot.Weapon)?.Name??"Mãos livres");
        }
        void RefreshGrid()
        {
            grid.Clear();int count=0;foreach(var stack in state.Progress.inventory.items){var item=ItemCatalog.Find(stack.id);if(item==null)continue;
                if(filter.index>0&&(int)item.Slot!=filter.index-1)continue;if(!string.IsNullOrWhiteSpace(search.value)&&item.Name.IndexOf(search.value,StringComparison.OrdinalIgnoreCase)<0)continue;
                var button=new Button(()=>Select(item.Id));button.name="item-"+item.Id;button.AddToClassList("item-cell");button.EnableInClassList("selected",item.Id==selected);
                var icon=new Image{sprite=art.Icons[item.Icon],scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};icon.AddToClassList("item-icon");button.Add(icon);
                bool equipped=InventoryRules.Equipped(state.Progress.inventory,item.Slot)?.Id==item.Id;
                button.Add(new Label(item.Name+(equipped?"\n✓ Equipado":"")){pickingMode=PickingMode.Ignore});grid.Add(button);count++;
            }
            root.Q<Label>("inventoryCount").text=count+" itens exibidos · "+state.Progress.inventory.items.Count+" / 64 espaços";
        }
        void RefreshDetails()
        {
            var item=ItemCatalog.Find(selected);bool valid=item!=null;
            root.Q<Button>("equipSelected").SetEnabled(valid && InventoryRules.Equipped(state.Progress.inventory,item.Slot)?.Id!=selected);
            root.Q<Button>("unequipSelected").SetEnabled(valid&&InventoryRules.Equipped(state.Progress.inventory,item.Slot)?.Id==selected);
            if(!valid){root.Q<Label>("detailName").text="Inventário vazio";root.Q<Image>("detailIcon").sprite=null;foreach(string field in new[]{"detailRarity","detailDescription","detailComparison"})root.Q<Label>(field).text="";return;}
            root.Q<Image>("detailIcon").sprite=art.Icons[item.Icon];root.Q<Label>("detailName").text=item.Name;
            root.Q<Label>("detailRarity").text=ItemCatalog.Rarities[item.Rarity]+" · "+ItemCatalog.Slots[(int)item.Slot];root.Q<Label>("detailDescription").text=item.Description;
            var current=InventoryRules.Stats(state.Progress.inventory);var next=InventoryRules.Stats(state.Progress.inventory,item.Id);
            root.Q<Label>("detailComparison").text="AO EQUIPAR\n\n"+Compare("Vida máxima",current.Health,next.Health)+Compare("Fôlego máximo",current.Stamina,next.Stamina)+Compare("Poder físico",current.Physical,next.Physical)+Compare("Defesa",current.Defense,next.Defense)+"\nPeso da peça: "+item.Bonus.Weight.ToString("0.0")+" kg";
            root.Q<Button>("equipSelected").text=InventoryRules.Equipped(state.Progress.inventory,item.Slot)?.Id==selected?"Já equipado":"Equipar";
        }
        static string Compare(string name,int before,int after){int d=after-before;return name+"  "+before+" → "+after+(d==0?"":"  ("+(d>0?"+":"")+d+")")+"\n";}
        void RefreshSlots(){var panel=root.Q("loadoutPage");panel.Clear();for(int i=0;i<8;i++){
            var slot=(GearSlot)i;var item=InventoryRules.Equipped(state.Progress.inventory,slot);var row=new VisualElement();row.AddToClassList("slot-row");
            row.Add(new Image{sprite=item==null?null:art.Icons[item.Icon]});row.Add(new Label(ItemCatalog.Slots[i]+"\n"+(item?.Name??"Vazio")));
            var change=new Button(()=>{filter.index=(int)slot+1;ShowPage("inventory");if(item!=null)Select(item.Id);}){text="Trocar"};row.Add(change);
            if(item!=null)row.Add(new Button(()=>{InventoryRules.Unequip(state.Progress.inventory,slot);Changed("Espaço liberado: "+ItemCatalog.Slots[(int)slot]);}){text="Retirar"});panel.Add(row);
        }}
        void RefreshStats(){var panel=root.Q("statusPage");panel.Clear();var s=InventoryRules.Stats(state.Progress.inventory);
            string[] rows={"ATRIBUTOS", "Vitalidade  "+s.Vitality,"Força  "+s.Strength,"Destreza  "+s.Dexterity,"Inteligência  "+s.Intelligence,"Vigor  "+s.Vigor,"VALORES DERIVADOS","Vida máxima  "+s.Health,"Fôlego máximo  "+s.Stamina,"Poder físico  "+s.Physical,"Defesa  "+s.Defense,"Peso equipado  "+s.Weight.ToString("0.0")+" kg"};
            foreach(var value in rows){var label=new Label(value);label.AddToClassList("stat-row");panel.Add(label);}
            foreach(var set in ItemCatalog.Sets){int count=InventoryRules.SetCount(state.Progress.inventory,set);var label=new Label(set+"  "+count+"/6 · "+(count>=3?"+2 vigor":"3 peças: +2 vigor")+" · "+(count==6?"+5 defesa":"6 peças: +5 defesa"));label.AddToClassList("stat-row");panel.Add(label);}
            var note=new Label("O fôlego já afeta exploração. Vida, poder e defesa estão preparados para a próxima etapa de combate.");note.AddToClassList("muted");note.AddToClassList("wrap");panel.Add(note);
        }
        public void Dispose(){root.RemoveFromHierarchy();heldSword.Dispose();preview.Dispose();camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(camera.gameObject);}
    }
}
