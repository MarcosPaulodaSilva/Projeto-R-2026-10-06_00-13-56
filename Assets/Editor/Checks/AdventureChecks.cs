using System;
namespace Vadronia
{
    public static class AdventureChecks
    {
        public static int Run(Action<string> report)
        {
            int count=CoreChecks.Run(report);
            Action<bool,string> require=(ok,name)=>{if(!ok)throw new Exception(name);report("PASS: "+name);count++;};
            var state=new AdventureState();
            require(state.TryDodge()&&!state.TryDodge()&&state.Stamina==76,"Esquiva gasta fôlego uma vez e respeita recarga");
            for(int i=0;i<8;i++)state.Tick(.1f,false);
            require(state.TryDodge(),"Esquiva volta após recarga");
            for(int i=0;i<150;i++)state.Tick(.1f,true);
            require(state.Stamina>=0&&state.Stamina<=100,"Corrida não produz fôlego negativo");
            for(int i=0;i<100;i++)state.Tick(.1f,false);
            require(state.Stamina==100,"Fôlego regenera até o limite");
            require(!state.Gather(0)&&!state.ClaimReward(),"Missão não progride antes da conversa");
            state.AcceptQuest();require(state.Gather(0)&&!state.Gather(0)&&!state.Gather(-1),"Canteiro não pode ser coletado duas vezes");
            state.Gather(1);state.Gather(2);
            require(state.ClaimReward()&&!state.ClaimReward()&&state.Progress.coins==25,"Recompensa concedida exatamente uma vez");
            var restored=new AdventureState();restored.Restore(new AdventureProgress{quest=state.Progress.quest,herbs=state.Progress.herbs,coins=state.Progress.coins});
            require(restored.HerbCount==3&&restored.Progress.quest==2&&!restored.ClaimReward(),"Progresso restaurado não duplica recompensa");
            int[] xs={0,1,1,1,0,-1,-1,-1},ys={-1,-1,0,1,1,1,0,-1};bool compass=true;
            for(int i=0;i<8;i++)compass&=EightDirection.Resolve(xs[i],ys[i])==i;
            require(compass,"Movimento distingue as oito direções");
            require(EightDirection.Resolve(.1f,-1)==0&&EightDirection.Resolve(1,.1f)==2&&EightDirection.Resolve(-1,-.1f)==6,
                "Desvio pequeno conserva direção cardinal sem oscilar para diagonal");
            return count;
        }
    }
}
