using System;
using System.Collections.Generic;
using UnityEngine;
namespace Vadronia
{
    public sealed class VillageAtmosphere : IDisposable
    {
        readonly GameObject root=new GameObject("Atmosfera de Grünwald");
        readonly VisualLibrary art=new VisualLibrary();
        readonly List<SpriteRenderer> leaves=new List<SpriteRenderer>(), smoke=new List<SpriteRenderer>(), lights=new List<SpriteRenderer>();
        readonly List<Transform> trees=new List<Transform>();
        readonly SpriteRenderer[] dust=new SpriteRenderer[14];
        readonly float[] dustLife=new float[14];
        readonly Vector2[] seeds=new Vector2[42];
        float clock,stepDistance;FootPoint previous;int nextDust;
        public VillageAtmosphere(TownWorld town,FootPoint start)
        {
            previous=start;
            foreach(var renderer in town.Root.GetComponentsInChildren<SpriteRenderer>())
                if(renderer.name.StartsWith("Bosque")||renderer.name.Contains("Carvalho")||renderer.name.Contains("Pinheiro"))trees.Add(renderer.transform);
            var random=new System.Random(86);
            for(int i=0;i<42;i++)
            {
                seeds[i]=new Vector2((float)random.NextDouble()*28-14,(float)random.NextDouble()*22-11);
                var leaf=art.Add(root.transform,"Folha ao vento "+i,art.Disc,seeds[i],new Vector2(.045f,.095f),new Color(.69f,.65f,.3f,.7f),15000);leaves.Add(leaf);
            }
            for(int i=0;i<12;i++) smoke.Add(art.Add(root.transform,"Fumaça da ferraria "+i,art.SoftDisc,new Vector2(7.95f,7.4f),Vector2.one,new Color(.7f,.72f,.67f,.25f),15000));
            foreach(var p in new[]{new Vector2(-5.3f,-3.4f),new Vector2(5.3f,-3.4f),new Vector2(-5.5f,3.6f),new Vector2(6.4f,3.6f),new Vector2(1.3f,-8.6f),new Vector2(-10.6f,4.8f)})
            {
                int order=-Mathf.RoundToInt(p.y*100)*10;
                lights.Add(art.Add(root.transform,"Luz quente",art.SoftDisc,p+Vector2.up*.72f,new Vector2(.85f,.85f),new Color(1,.64f,.23f,.2f),order+2));
            }
            for(int i=0;i<dust.Length;i++){dust[i]=art.Add(root.transform,"Poeira de passo "+i,art.SoftDisc,Vector2.zero,Vector2.one,new Color(1,.85f,.62f,0),-7000);}
        }
        public void Tick(float dt,CharacterView player,bool sprinting)
        {
            clock+=dt;
            foreach(var tree in trees)tree.localRotation=Quaternion.Euler(0,0,Mathf.Sin(clock*.9f+tree.position.x)*.5f);
            for(int i=0;i<leaves.Count;i++)
            {
                float x=Mathf.Repeat(seeds[i].x+14+clock*.23f,28)-14;
                float y=Mathf.Repeat(seeds[i].y+11-clock*.12f,22)-11;
                leaves[i].transform.localPosition=new Vector3(x+Mathf.Sin(clock+i)*.2f,y,0);
                leaves[i].transform.localRotation=Quaternion.Euler(0,0,clock*45+i*27);
            }
            for(int i=0;i<smoke.Count;i++)
            {
                float life=Mathf.Repeat(clock*.23f+i/12f,1);
                smoke[i].transform.localPosition=new Vector3(8.4f+life*.6f+Mathf.Sin(clock+i)*.1f,6.4f+life*2.6f,0);
                smoke[i].transform.localScale=Vector3.one*(.25f+life*1.4f);
                smoke[i].color=new Color(.72f,.73f,.69f,Mathf.Sin(life*Mathf.PI)*.25f);
            }
            foreach(var light in lights)light.color=new Color(1,.67f,.3f,.34f+Mathf.Sin(clock*3+light.transform.position.x)*.045f);
            var p=player.Position;stepDistance+=Vector2.Distance(new Vector2(previous.X,previous.Y),new Vector2(p.X,p.Y));previous=p;
            if(player.Cycle.Moving&&stepDistance>.28f){stepDistance=0;int i=nextDust++%dust.Length;dustLife[i]=1;dust[i].transform.localPosition=new Vector3(p.X,p.Y+.015f,0);}
            for(int i=0;i<dust.Length;i++)if(dustLife[i]>0)
            {
                dustLife[i]=Mathf.Max(0,dustLife[i]-dt*2);float t=1-dustLife[i];
                dust[i].transform.localScale=new Vector3(.23f+t*.55f,.09f+t*.22f,1);
                dust[i].color=new Color(.92f,.81f,.59f,dustLife[i]*(sprinting?.32f:.18f));
            }
        }
        public void Dispose(){UnityEngine.Object.Destroy(root);art.Dispose();}
    }
}
