using System;
using UnityEngine;
using UnityEditor;
namespace Vadronia.Editor
{
    public static class ReadabilityChecks
    {
        [MenuItem("Vadronia/Verificar visibilidade")]
        public static void Run()
        {
            var game=UnityEngine.Object.FindAnyObjectByType<VadroniaDemo>();
            if(!Application.isPlaying||game==null)throw new Exception("Entre em Play.");
            int count=0;Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);count++;};
            var root=new GameObject("Teste de visibilidade");
            var tree=new GameObject("Bosque teste").AddComponent<SpriteRenderer>();tree.transform.SetParent(root.transform);
            var actor=new GameObject("Ator teste").AddComponent<SpriteRenderer>();
            var camera=new GameObject("Camera teste").AddComponent<Camera>();camera.enabled=false;camera.aspect=16f/9;
            try{
                tree.sprite=actor.sprite=game.Player.Transform.GetComponent<SpriteRenderer>().sprite;
                tree.sortingOrder=20;actor.sortingOrder=10;
                var system=new CharacterVisibility(root.transform,new[]{actor});
                system.Tick(1);check(Mathf.Abs(tree.color.a-.35f)<.001f,"Árvore revela ator atrás");
                actor.sortingOrder=30;system.Tick(1);check(tree.color.a==1,"Ator à frente não apaga árvore");
                actor.sortingOrder=10;actor.transform.position=Vector3.right*10;system.Tick(1);check(tree.color.a==1,"Fora da copa permanece opaca");
                actor.transform.position=Vector3.zero;actor.enabled=false;system.Tick(1);check(tree.color.a==1,"Ator oculto não altera copa");
                var rig=new AdventureCamera(camera);
                rig.Follow(new FootPoint(0,0),Vector2.zero,false,.02f,true);float normal=camera.orthographicSize;
                for(int i=0;i<60;i++)rig.Follow(new FootPoint(0,0),Vector2.up,true,.02f);
                check(Mathf.Abs(normal-camera.orthographicSize)<.001f,"Corrida mantém escala");
                check(1.02f*640/(2*normal)>=90,"Personagem ocupa ao menos 90 pixels em tela de 640");
                rig.Follow(new FootPoint(100,100),Vector2.one,false,.02f,true);
                check(camera.transform.position.x+normal*camera.aspect<=14.001f&&camera.transform.position.y+normal<=11.001f,"Enquadramento permanece no mapa");
                Debug.Log("READABILITY PASS: "+count+" verificações.");
            }finally{UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(actor.gameObject);UnityEngine.Object.Destroy(camera.gameObject);}
        }
    }
}
