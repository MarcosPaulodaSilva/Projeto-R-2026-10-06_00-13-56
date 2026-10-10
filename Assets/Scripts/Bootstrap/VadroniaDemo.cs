using UnityEngine;
using UnityEngine.UIElements;
namespace Vadronia
{
    /// <summary>Composition root for the village, presentation and exploration systems.</summary>
    public sealed class VadroniaDemo : MonoBehaviour
    {
        public Texture2D characterAtlas;
        public CharacterView Player => player;
        public CharacterView Conrad => conrad;
        public ExplorerMotor Motor => motor;
        public PlayerSword Sword => sword;
        public VillageMusic Music => music;
        public AdventureHud Hud => hud;
        public VillageInteraction Interactions => interactions;
        CharacterView player,conrad;TownWorld town;Camera view;ExplorerMotor motor;PlayerSword sword;
        AdventureCamera cameraRig;AdventureHud hud;VillageInteraction interactions;VillageAtmosphere atmosphere;VillageNpcs villagers;VillageCrowd crowd;VillageMusic music;
        EquipmentArt gearArt;EquipmentWindow gearWindow;
        public EquipmentWindow Equipment => gearWindow;
        int waypoint=1;float wait;bool warnedPatrol;
        void Start()
        {
            var scenery=Resources.Load<Texture2D>("Vadronia/town");
            if(scenery==null||characterAtlas==null){Debug.LogError("Arte de Vadronia ausente",this);enabled=false;return;}
            QualitySettings.antiAliasing=0;QualitySettings.anisotropicFiltering=AnisotropicFiltering.Disable;Application.targetFrameRate=60;
            town=new TownWorld(scenery);
            player=new CharacterView("Player",new FootPoint(0,-3.9f),null,characterAtlas,false);
            conrad=new CharacterView("Konrad",TownLayout.Patrol[0],null,characterAtlas,true);
            view=new GameObject("Camera — Grünwald").AddComponent<Camera>();view.tag="MainCamera";view.orthographic=true;view.orthographicSize=5.65f;
            view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color32(34,48,35,255);view.allowHDR=view.allowMSAA=view.allowDynamicResolution=false;
            // A cena de demo não tem AudioListener; a câmera criada em runtime precisa de um.
            if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
                view.gameObject.AddComponent<AudioListener>();
            cameraRig=new AdventureCamera(view);motor=new ExplorerMotor(player,town);sword=null;
            hud=new AdventureHud();interactions=new VillageInteraction(motor,conrad,hud);
            gearArt=new EquipmentArt();
            gearWindow=new EquipmentWindow(hud.DocumentRoot,motor.State,gearArt,interactions.SaveEquipment);
            hud.DocumentRoot.Q<Button>("openEquipment").clicked+=()=>{if(!hud.Paused&&!hud.DialogOpen)gearWindow.SetOpen(true);};
            view.cullingMask=~(1<<30);
            music=new VillageMusic();
            hud.MusicToggleRequested=()=>{music.ToggleMuted();hud.SetMusicMuted(music.Muted);};
            hud.SetMusicMuted(music.Muted);
            villagers=new VillageNpcs(town.Blocks);interactions.Npcs=villagers;
            crowd=new VillageCrowd(town.Blocks);
            atmosphere=new VillageAtmosphere(town,player.Position);
            cameraRig.Follow(player.Position,Vector2.zero,false,0,true);
        }
        void Update()
        {
            if(motor==null||hud==null)return;
            float dt=Mathf.Min(Time.deltaTime,.05f);
            bool wasOpen=gearWindow.IsOpen;
            gearWindow.HandleInput(!hud.Paused&&!hud.DialogOpen);
            hud.ModalOpen=wasOpen||gearWindow.IsOpen;
            hud.Tick(motor.State,interactions.Hint,Time.unscaledDeltaTime);
            hud.ModalOpen=gearWindow.IsOpen;
            music?.Tick(Time.unscaledDeltaTime, hud.Paused||gearWindow.IsOpen);
            if(wasOpen||gearWindow.IsOpen){motor.Halt();return;}
            if(hud.Paused){sword?.Interrupt();motor.Halt();return;}
            interactions.Tick(dt);
            if(hud.DialogOpen)
            {
                sword?.Interrupt();motor.Halt();conrad.Place(conrad.Position);conrad.Animate(dt);
                villagers?.Tick(0,true,new Vector2(player.Position.X,player.Position.Y));
                crowd?.Tick(0,true);
                return;
            }
            motor.Tick(dt,sword==null||sword.Combo.CanAct);sword?.Tick(dt);TickPatrol(dt);
            villagers?.Tick(dt,false,new Vector2(player.Position.X,player.Position.Y));
            crowd?.Tick(dt,false);
            atmosphere.Tick(dt,player,motor.IsSprinting);
        }
        void TickPatrol(float dt)
        {
            var old=conrad.Position;
            if(wait>0){wait-=dt;conrad.Place(old);conrad.Animate(dt);return;}
            var target=TownLayout.Patrol[waypoint];
            var movement=Vector2.ClampMagnitude(new Vector2(target.X-old.X,target.Y-old.Y),1.35f*dt);
            var next=MovementCore.Move(old,movement.x,movement.y,town.Blocks);conrad.Place(next);conrad.Animate(dt);
            if(Vector2.Distance(new Vector2(next.X,next.Y),new Vector2(target.X,target.Y))<.02f){int reached=waypoint;waypoint=(waypoint+1)%TownLayout.Patrol.Length;wait=TownLayout.PatrolPause[reached];}
            else if(!conrad.Cycle.Moving&&!warnedPatrol){warnedPatrol=true;Debug.LogWarning("Konrad encontrou obstáculo na patrulha.");}
        }
        void LateUpdate(){if(cameraRig!=null&&hud!=null&&!hud.Paused&&!gearWindow.IsOpen){Vector2 look=sword!=null&&sword.Combo.Busy?sword.Aim:motor.Heading;cameraRig.Follow(player.Position,look,motor.IsSprinting,Mathf.Min(Time.deltaTime,.05f));}}
        void OnDestroy(){gearWindow?.Dispose();gearArt?.Dispose();interactions?.Dispose();hud?.Dispose();atmosphere?.Dispose();villagers?.Dispose();crowd?.Dispose();music?.Dispose();sword?.Dispose();player?.Dispose();conrad?.Dispose();town?.Dispose();if(view!=null)Destroy(view.gameObject);}
        void OnDrawGizmosSelected()
        {
            Gizmos.color=new Color(1,.5f,0,.7f);
            foreach(var b in TownLayout.Blocks())Gizmos.DrawWireCube(new Vector3((b.Left+b.Right)/2,(b.Bottom+b.Top)/2,0),new Vector3(b.Right-b.Left,b.Top-b.Bottom,0));
        }
    }
}
