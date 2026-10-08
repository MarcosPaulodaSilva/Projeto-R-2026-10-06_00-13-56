using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Vadronia
{
    public sealed class AdventureHud : IDisposable
    {
        readonly GameObject root;
        readonly PanelSettings panel;
        readonly VisualElement hud,dialog,pause;
        readonly Label stamina,coins,questTitle,questText,prompt,toast,speaker,dialogText;
        readonly VisualElement[] segments=new VisualElement[10];
        float toastTime;
        public bool Paused {get;private set;}
        public bool DialogOpen {get;private set;}
        public bool Hidden {get;private set;}
        public Action SaveRequested;
        public AdventureHud()
        {
            root=new GameObject("Interface — Vadronia");
            panel=Resources.Load<PanelSettings>("Vadronia/UI/AdventurePanel");
            if(panel==null)throw new InvalidOperationException("AdventurePanel ausente. Execute Vadronia/Preparar interface.");
            var document=root.AddComponent<UIDocument>();document.panelSettings=panel;
            var tree=Resources.Load<VisualTreeAsset>("Vadronia/UI/AdventureHud");document.visualTreeAsset=tree;
            hud=document.rootVisualElement.Q("hud");dialog=hud.Q("dialog");pause=hud.Q("pause");
            stamina=hud.Q<Label>("staminaText");coins=hud.Q<Label>("coins");questTitle=hud.Q<Label>("questTitle");questText=hud.Q<Label>("questText");
            prompt=hud.Q<Label>("prompt");toast=hud.Q<Label>("toast");speaker=hud.Q<Label>("speaker");dialogText=hud.Q<Label>("dialogText");
            for(int i=0;i<10;i++){segments[i]=new VisualElement();segments[i].AddToClassList("segment");hud.Q("segments").Add(segments[i]);}
            hud.Q<Button>("resume").clicked+=()=>SetPause(false);hud.Q<Button>("save").clicked+=()=>SaveRequested?.Invoke();
            foreach(var label in hud.Query<Label>().ToList())label.pickingMode=PickingMode.Ignore;
        }
        public void Tick(AdventureState state,string hint,float dt)
        {
            if(ExplorerMotor.Pressed(KeyCode.F1)&&!DialogOpen&&!Paused){Hidden=!Hidden;hud.EnableInClassList("hidden",Hidden);}
            if(ExplorerMotor.Pressed(KeyCode.Escape)){if(DialogOpen)CloseDialog();else SetPause(!Paused);}
            if(ExplorerMotor.Pressed(KeyCode.F5))SaveRequested?.Invoke();
            stamina.text="FÔLEGO  "+Mathf.RoundToInt(state.Stamina)+" / 100";
            for(int i=0;i<10;i++)segments[i].EnableInClassList("empty",state.Stamina<(i+1)*10);
            coins.text=state.Progress.coins+" moedas  ·  "+(state.Dodging?"Esquiva":"Exploração");
            string title,text;QuestJournal.Describe(state,out title,out text);
            questTitle.text=title;questText.text=text;
            prompt.text=hint;prompt.EnableInClassList("hidden",string.IsNullOrEmpty(hint)||DialogOpen||Paused);
            toastTime-=dt;toast.EnableInClassList("hidden",toastTime<=0);
        }
        public void Notify(string text){toast.text=text;toastTime=4;}
        public void ShowDialog(string name,string text){Hidden=false;hud.RemoveFromClassList("hidden");speaker.text=name;dialogText.text=text;DialogOpen=true;dialog.RemoveFromClassList("hidden");}
        public void CloseDialog(){DialogOpen=false;dialog.AddToClassList("hidden");}
        public void SetPause(bool value){Paused=value;Hidden=false;hud.RemoveFromClassList("hidden");pause.EnableInClassList("hidden",!value);}
        public void Dispose(){UnityEngine.Object.Destroy(root);}
    }
}
