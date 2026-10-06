using UnityEngine;
namespace Vadronia
{
    public sealed class AdventureCamera
    {
        readonly Camera view;
        Vector3 velocity;
        public AdventureCamera(Camera view){this.view=view;}
        public void Follow(FootPoint position,Vector2 heading,bool sprinting,float dt,bool snap=false)
        {
            float zoom=sprinting?6.15f:5.65f;
            view.orthographicSize=snap?zoom:Mathf.Lerp(view.orthographicSize,zoom,1-Mathf.Exp(-dt*4));
            float width=view.orthographicSize*view.aspect;
            float x=position.X+heading.x*.32f,y=position.Y+1+heading.y*.15f;
            x=width>=14?0:Mathf.Clamp(x,-14+width,14-width);
            y=Mathf.Clamp(y,-11+view.orthographicSize,11-view.orthographicSize);
            Vector3 target=new Vector3(x,y,-10);
            view.transform.position=snap?target:Vector3.SmoothDamp(view.transform.position,target,ref velocity,.12f,100,dt);
        }
    }
}
