using System.Collections.Generic;
using UnityEngine;
namespace Vadronia
{
    public sealed class CharacterVisibility
    {
        readonly List<SpriteRenderer> trees=new List<SpriteRenderer>();
        readonly SpriteRenderer[] characters;
        public CharacterVisibility(Transform town,SpriteRenderer[] characters)
        {
            this.characters=characters;
            var names=new HashSet<string>();
            foreach(var prop in TownLayout.Props)if(prop.IsTree)names.Add(prop.Name);
            foreach(var sprite in town.GetComponentsInChildren<SpriteRenderer>())
                if(names.Contains(sprite.name)||sprite.name.StartsWith("Bosque "))trees.Add(sprite);
        }
        public static bool Covers(SpriteRenderer tree,SpriteRenderer character)
        {
            return character!=null&&character.enabled&&tree.sortingOrder>character.sortingOrder
                &&tree.bounds.Intersects(character.bounds);
        }
        public void Tick(float dt)
        {
            foreach(var tree in trees){
                bool covered=false;
                foreach(var character in characters)if(Covers(tree,character)){covered=true;break;}
                var color=tree.color;color.a=Mathf.MoveTowards(color.a,covered?.35f:1f,Mathf.Max(0,dt)*4);
                tree.color=color;
            }
        }
    }
}
