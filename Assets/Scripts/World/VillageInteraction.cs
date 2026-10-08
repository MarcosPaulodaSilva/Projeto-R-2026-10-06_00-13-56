using System;
using System.IO;
using UnityEngine;
namespace Vadronia
{
    public sealed class VillageInteraction : IDisposable
    {
        readonly ExplorerMotor motor;readonly CharacterView conrad;readonly AdventureHud hud;
        readonly GameObject root=new GameObject("Interações de Grünwald");readonly VisualLibrary art=new VisualLibrary();
        readonly Vector2[] herbs={new Vector2(-5.5f,-2),new Vector2(8,-6.5f),new Vector2(-10.2f,7.2f)};
        readonly GameObject[] plants=new GameObject[3];readonly SpriteRenderer marker;
        readonly string savePath;
        float clock; int target=-1;
        public string Hint {get;private set;}="";
        public VillageNpcs Npcs;
        public VillageInteraction(ExplorerMotor motor,CharacterView conrad,AdventureHud hud)
        {
            this.motor=motor;this.conrad=conrad;this.hud=hud;
            savePath=Path.Combine(Application.persistentDataPath,"grunwald-adventure-v1.json");Load();
            for(int i=0;i<herbs.Length;i++)
            {
                plants[i]=new GameObject("Ervas do jardim "+(i+1));plants[i].transform.SetParent(root.transform);plants[i].transform.position=herbs[i];
                for(int j=0;j<7;j++)
                {
                    float angle=j*Mathf.PI*2/7;Vector2 p=new Vector2(Mathf.Cos(angle)*.15f,.1f+Mathf.Sin(angle)*.09f);
                    var leaf=art.Add(plants[i].transform,"Folha",art.Disc,p,new Vector2(.12f,.27f),new Color(.48f,.69f,.31f),-Mathf.RoundToInt(herbs[i].y*100)*10+5);
                    leaf.transform.localRotation=Quaternion.Euler(0,0,-angle*Mathf.Rad2Deg);
                }
                art.Add(plants[i].transform,"Sinal de coleta",art.SoftDisc,new Vector2(0,.1f),new Vector2(.8f,.5f),new Color(.9f,.79f,.36f,.5f),14000);
                plants[i].SetActive((motor.State.Progress.herbs&(1<<i))==0);
            }
            marker=art.Add(root.transform,"Konrad — conversa",art.Square,Vector2.zero,new Vector2(.12f,.12f),new Color(1,.81f,.37f),15000);
            marker.transform.localRotation=Quaternion.Euler(0,0,45);
            hud.SaveRequested=Save;hud.Notify("Bem-vindo a Grünwald. Encontre Konrad na praça.");
        }
        public void Tick(float dt)
        {
            clock+=dt;marker.transform.position=new Vector3(conrad.Position.X,conrad.Position.Y+2.04f+Mathf.Sin(clock*2)*.06f,0);
            Vector2 p=new Vector2(motor.Actor.Position.X,motor.Actor.Position.Y);
            int npcIndex;
            target=-1;Hint="";
            if(Vector2.Distance(p,new Vector2(conrad.Position.X,conrad.Position.Y))<1.6f){target=0;Hint="T  ·  Conversar com Konrad";}
            else if(Vector2.Distance(p,new Vector2(0,-.3f))<1.25f){target=1;Hint="T  ·  Beber água e recuperar fôlego";}
            else for(int i=0;i<herbs.Length;i++)if(motor.State.Progress.quest==1&&(motor.State.Progress.herbs&(1<<i))==0&&Vector2.Distance(p,herbs[i])<1.1f){target=2+i;Hint="T  ·  Colher ervas do jardim";break;}
            if(target<0&&Vector2.Distance(p,new Vector2(-3,4.25f))<1.3f){target=5;Hint="T  ·  Descansar na estalagem";}
            if(target<0&&Vector2.Distance(p,new Vector2(4.3f,4.25f))<1.3f){target=GrunwaldStory.GuildaChoice;Hint="T  ·  Ler a placa da guilda";}
            if(target<0)foreach(var point in GrunwaldStory.Points)
                if(Vector2.Distance(p,new Vector2(point.X,point.Y))<point.Radius){target=point.Choice;Hint=point.Hint;break;}
            if(target<0&&Npcs!=null&&(npcIndex=Npcs.Nearest(p,out string npcHint))>=0){target=100+npcIndex;Hint=npcHint;}
            if(!ExplorerMotor.Pressed(KeyCode.T))return;
            if(hud.DialogOpen){hud.CloseDialog();return;}
            if(hud.Paused)return;
            Interact(target);
        }
        public void Interact(int choice)
        {
            if(choice==0)
            {
                conrad.Face(new Vector2(motor.Actor.Position.X-conrad.Position.X,motor.Actor.Position.Y-conrad.Position.Y));
                if(motor.State.Progress.quest==0){motor.State.AcceptQuest();hud.ShowDialog("KONRAD","Bem-vindo a Grünwald. Preciso de três porções de ervas para os viajantes. Há canteiros a oeste da praça, perto da banca ao sul e no jardim a noroeste. Pode me ajudar?");Save(false);}
                else if(motor.State.ClaimReward()){hud.ShowDialog("KONRAD","São as ervas de que precisávamos. Obrigado pela ajuda! Aqui estão 25 moedas pelo seu trabalho. Aproveite a vila.");Save(false);}
                else if(motor.State.Progress.quest==2)TalkAboutTheStone();
                else hud.ShowDialog("KONRAD","Os três canteiros ficam a oeste da praça, a sudeste e a noroeste. Chegue perto das ervas e pressione T. Volte quando tiver as três porções.");
            }
            else if(choice==1||choice==5){motor.State.Rest();hud.Notify(choice==1?"Água fresca. Fôlego recuperado.":"Um breve descanso. Fôlego recuperado.");Save(false);}
            else if(choice>=2&&choice<=4&&motor.State.Gather(choice-2)){plants[choice-2].SetActive(false);hud.Notify("Ervas colhidas  ·  "+motor.State.HerbCount+" / 3");Save(false);}
            else if(choice==GrunwaldStory.GuildaChoice)Hear(StoryFlags.VozesGuilda,GrunwaldStory.GuildaSpeaker,GrunwaldStory.GuildaText);
            else if(choice>=100&&Npcs!=null){Npcs.FacePlayer(choice-100,new Vector2(motor.Actor.Position.X,motor.Actor.Position.Y));if(Npcs.Talk(choice-100,motor.State,hud))Save(false);}
            else foreach(var point in GrunwaldStory.Points)if(point.Choice==choice){Hear(point.Flag,point.Speaker,point.Text);break;}
        }
        // Konrad after the herb favour: offer "Vozes da vila", report progress, pay once, then comment.
        void TalkAboutTheStone()
        {
            var state=motor.State;
            if(Vozes.TryAccept(state)){hud.ShowDialog("KONRAD",GrunwaldStory.KonradOferta(Vozes.Heard(state)));Save(false);}
            else if(Vozes.TryComplete(state)){hud.ShowDialog("KONRAD",GrunwaldStory.KonradConclusao());hud.Notify("Relatos anotados  ·  +"+Vozes.Reward+" moedas");Save(false);}
            else if(state.HasFlag(StoryFlags.VozesConcluida))hud.ShowDialog("KONRAD",GrunwaldStory.KonradDepois);
            else hud.ShowDialog("KONRAD",GrunwaldStory.KonradAndamento(state.HasFlag(StoryFlags.VozesMural),state.HasFlag(StoryFlags.VozesBanca),state.HasFlag(StoryFlags.VozesGuilda)));
        }
        // Reading a notice or overhearing talk is world knowledge: it is remembered even before Konrad asks.
        void Hear(string flag,string speaker,string text)
        {
            bool fresh=motor.State.AddFlag(flag);
            hud.ShowDialog(speaker,text);
            if(!fresh)return;
            Save(false);
            if(Vozes.Active(motor.State))hud.Notify("Relato ouvido  ·  "+Vozes.Heard(motor.State)+" / "+Vozes.Needed);
        }
        public void Save(){Save(true);}
        public void RefreshPlants()
        {
            for(int i=0;i<plants.Length;i++)plants[i].SetActive((motor.State.Progress.herbs&(1<<i))==0);
        }
        void Save(bool notify)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));string temp=savePath+".tmp";
                File.WriteAllText(temp,JsonUtility.ToJson(motor.State.Progress,true));
                if(File.Exists(savePath))File.Replace(temp,savePath,savePath+".previous");else File.Move(temp,savePath);
                if(notify)hud.Notify("Progresso salvo.");
            }
            catch(Exception e){Debug.LogWarning("Não foi possível salvar: "+e.Message);hud.Notify("Não foi possível salvar o progresso.");}
        }
        void Load()
        {
            if(!File.Exists(savePath))return;
            try{motor.State.Restore(JsonUtility.FromJson<AdventureProgress>(File.ReadAllText(savePath)));}
            catch(Exception e){Debug.LogWarning("Save não carregado; original preservado: "+e.Message);}
        }
        public void Dispose(){UnityEngine.Object.Destroy(root);art.Dispose();}
    }
}
