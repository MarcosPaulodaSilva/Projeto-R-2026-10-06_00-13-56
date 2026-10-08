using System;
using System.Collections.Generic;
namespace Vadronia
{
    /// <summary>Short-range, facing-aware selection shared by every interaction category.</summary>
    public static class InteractionFocus
    {
        public static float Score(FootPoint player, FootPoint facing, FootPoint target, float range,
            bool frontOnly, IList<FootBlock> blocks, bool previous)
        {
            float dx=target.X-player.X,dy=target.Y-player.Y;
            float distance=(float)Math.Sqrt(dx*dx+dy*dy);
            if(distance>range || (frontOnly && (player.Y>target.Y+.12f || Math.Abs(dx)>.55f)))return float.PositiveInfinity;
            float dot=distance>.001f?(dx*facing.X+dy*facing.Y)/distance:1f;
            if(distance>.32f && dot<-.15f)return float.PositiveInfinity;
            // A ray through intervening footprints prevents interaction through fences/houses.
            int steps=Math.Max(1,(int)Math.Ceiling(distance/.08f));
            for(int i=1;i<=steps;i++)
                foreach(var block in blocks)
                    if(block.Contains(player.X+dx*i/steps,player.Y+dy*i/steps,.025f))return float.PositiveInfinity;
            return distance+.16f*(1-dot)-(previous?.055f:0);
        }
    }
}
