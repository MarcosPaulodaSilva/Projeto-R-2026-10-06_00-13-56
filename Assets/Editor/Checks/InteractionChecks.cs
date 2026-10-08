using System;
using System.Collections.Generic;
namespace Vadronia
{
    public static class InteractionChecks
    {
        public static int Run(Action<string> report)
        {
            int count=0;var blocks=TownLayout.Blocks();var up=new FootPoint(0,1);
            Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception(message);count++;report("PASS: "+message);};
            var board=new FootPoint(5.4f,2.57f);
            check(!float.IsInfinity(InteractionFocus.Score(new FootPoint(5.4f,2.05f),up,board,.7f,true,blocks,false)),"Mural acessível pela frente");
            check(float.IsInfinity(InteractionFocus.Score(new FootPoint(6.1f,2.57f),new FootPoint(-1,0),board,.7f,true,blocks,false)),"Mural não exige nem aceita acesso lateral distante");
            check(float.IsInfinity(InteractionFocus.Score(new FootPoint(5.4f,3.35f),new FootPoint(0,-1),board,.7f,true,blocks,false)),"Mural não interage por trás");
            check(float.IsInfinity(InteractionFocus.Score(new FootPoint(5.4f,1.55f),up,board,.7f,true,blocks,false)),"Alvo a mais de um metro não pode ser acionado");
            var empty=new List<FootBlock>();var player=new FootPoint(0,0);
            float near=InteractionFocus.Score(player,up,new FootPoint(0,.4f),.8f,false,empty,false);
            float far=InteractionFocus.Score(player,up,new FootPoint(0,.75f),.8f,false,empty,true);
            check(near<far,"Alvo próximo supera prioridade anterior distante");
            check(float.IsInfinity(InteractionFocus.Score(player,up,new FootPoint(0,-.6f),.8f,false,empty,false)),"Virar de costas descarta alvo fora do contato imediato");
            var wall=new List<FootBlock>{new FootBlock(-1,.25f,2,.1f)};
            check(float.IsInfinity(InteractionFocus.Score(player,up,new FootPoint(0,.6f),.8f,false,wall,false)),"Não é possível interagir através de parede");
            var ids=new HashSet<int>();bool doors=true;
            foreach(var b in VillageBuildings.All)
                doors &= ids.Add(b.Choice)&&MovementCore.Clear(b.Door.X,b.Door.Y,blocks)&&
                    !float.IsInfinity(InteractionFocus.Score(new FootPoint(b.Door.X,b.Door.Y-.25f),up,b.Door,.7f,true,blocks,false));
            check(doors,"Todos os edifícios têm IDs únicos e entrada frontal utilizável");
            return count;
        }
    }
}
