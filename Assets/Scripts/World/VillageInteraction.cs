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
        readonly SpriteRenderer focusMarker;
        readonly System.Collections.Generic.List<FootBlock> blocks=TownLayout.Blocks();
        readonly string savePath;
        float clock; int target=-1;
        public string Hint {get;private set;}="";
        public int Target => target;
        public FootPoint TargetPosition {get;private set;}
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
            focusMarker=art.Add(root.transform,"Alvo da interação",art.SoftDisc,Vector2.zero,new Vector2(.65f,.22f),new Color(.45f,.8f,1f,.75f),14001);
            hud.SaveRequested=Save;hud.Notify("Bem-vindo a Grünwald. Encontre Konrad na praça.");
        }
        public void Tick(float dt)
        {
            clock+=dt;marker.transform.position=new Vector3(conrad.Position.X,conrad.Position.Y+2.04f+Mathf.Sin(clock*2)*.06f,0);
            SelectTarget();
            if(!ExplorerMotor.Pressed(KeyCode.T))return;
            if(hud.DialogOpen){hud.CloseDialog();return;}
            if(hud.Paused)return;
            Interact(target);
        }
        public void SelectTarget()
        {
            int previous=target;target=-1;Hint="";float best=float.PositiveInfinity;
            var p=motor.Actor.Position;
            float angle=motor.Actor.Facing*Mathf.PI/4;
            var facing=new FootPoint(Mathf.Sin(angle),-Mathf.Cos(angle));
            Consider(0,conrad.Position,.8f,false,"T  ·  Conversar com Konrad",p,facing,previous,ref best);
            Consider(1,new FootPoint(0,-.34f),.65f,true,"T  ·  Beber água do poço",p,facing,previous,ref best);
            for(int i=0;i<herbs.Length;i++)
                if(motor.State.Progress.quest==1&&(motor.State.Progress.herbs&(1<<i))==0)
                    Consider(2+i,new FootPoint(herbs[i].x,herbs[i].y),.65f,false,"T  ·  Colher ervas",p,facing,previous,ref best);
            string place="";float near=2.6f;
            foreach(var building in VillageBuildings.All)
            {
                float d=Vector2.Distance(new Vector2(p.X,p.Y),new Vector2(building.Door.X,building.Door.Y));
                if(d<near&&p.Y<building.Door.Y+.4f){near=d;place=building.Name+"  ·  "+building.Function;}
                string verb=building.Choice==5?"Descansar na ":building.Choice==6?"Ler registro da ":"Identificar ";
                Consider(building.Choice,building.Door,.7f,true,"T  ·  "+verb+building.Name,p,facing,previous,ref best);
            }
            foreach(var point in GrunwaldStory.Points)
                Consider(point.Choice,new FootPoint(point.X,point.Y),point.Radius,true,point.Hint,p,facing,previous,ref best);
            // Other counters remain usable independently of the story-bearing east stall.
            Consider(22,new FootPoint(-3.3f,-2.23f),.7f,true,"T  ·  Banca de alimentos",p,facing,previous,ref best);
            Consider(23,new FootPoint(6.4f,-5.08f),.7f,true,"T  ·  Banca de ervas",p,facing,previous,ref best);
            if(Npcs!=null)for(int i=0;i<Npcs.Count;i++)
                Consider(100+i,Npcs.Position(i),.8f,false,Npcs.Hint(i),p,facing,previous,ref best);
            focusMarker.gameObject.SetActive(target>=0&&!hud.DialogOpen&&!hud.Paused);
            focusMarker.transform.position=new Vector3(TargetPosition.X,TargetPosition.Y,0);
            hud.SetInteractionHint(Hint);hud.SetPlace(place);
        }
        void Consider(int choice,FootPoint at,float range,bool front,string hint,FootPoint p,FootPoint facing,int previous,ref float best)
        {
            float score=InteractionFocus.Score(p,facing,at,range,front,blocks,previous==choice);
            if(score>=best)return;
            best=score;target=choice;Hint=hint;TargetPosition=at;
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
            else if(choice==22)hud.ShowDialog("BANCA DE ALIMENTOS","Frutas, legumes e pão dos moradores de Grünwald. Na banca leste circulam também notícias da estrada. O comércio ainda não está disponível.");
            else if(choice==23)hud.ShowDialog("BANCA DE ERVAS","Ervas e hortaliças da vila. Maren conhece os canteiros usados no favor de Konrad; converse com ela para saber onde procurar.");
            else if(choice>=20&&choice<100)
            {foreach(var building in VillageBuildings.All)if(building.Choice==choice){hud.ShowDialog(building.Name.ToUpperInvariant(),building.Text);break;}}
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
        public bool SaveEquipment(){return Save(false);}
        public void RefreshPlants()
        {
            for(int i=0;i<plants.Length;i++)plants[i].SetActive((motor.State.Progress.herbs&(1<<i))==0);
        }
        bool Save(bool notify)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));string temp=savePath+".tmp";
                File.WriteAllText(temp,JsonUtility.ToJson(motor.State.Progress,true));
                if(File.Exists(savePath))File.Replace(temp,savePath,savePath+".previous");else File.Move(temp,savePath);
                if(notify)hud.Notify("Progresso salvo.");return true;
            }
            catch(Exception e){Debug.LogWarning("Não foi possível salvar: "+e.Message);hud.Notify("Não foi possível salvar o progresso.");return false;}
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
