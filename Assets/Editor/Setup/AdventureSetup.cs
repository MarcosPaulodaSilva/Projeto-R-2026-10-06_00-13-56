using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Vadronia.Editor
{
    public static class AdventureSetup
    {
        [MenuItem("Vadronia/Preparar interface")]
        public static void Prepare()
        {
            const string path="Assets/Resources/Vadronia/UI/AdventurePanel.asset";
            var panel=AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if(panel==null){panel=ScriptableObject.CreateInstance<PanelSettings>();AssetDatabase.CreateAsset(panel,path);}
            panel.scaleMode=PanelScaleMode.ScaleWithScreenSize;panel.referenceResolution=new Vector2Int(1280,720);panel.match=.5f;
            const string fontPath="Assets/Resources/Vadronia/UI/AdventureFont.asset";
            var font=AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontPath);
            if(font==null)
            {
                font=UnityEngine.TextCore.Text.FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Vadronia/UI/Inter-Regular.ttf"));
                font.name="AdventureFont";AssetDatabase.CreateAsset(font,fontPath);
                if(font.material!=null)AssetDatabase.AddObjectToAsset(font.material,font);
                foreach(var atlas in font.atlasTextures)if(atlas!=null)AssetDatabase.AddObjectToAsset(atlas,font);
            }
            const string settingsPath="Assets/Resources/Vadronia/UI/AdventureText.asset";
            var settings=AssetDatabase.LoadAssetAtPath<PanelTextSettings>(settingsPath);
            if(settings==null){settings=ScriptableObject.CreateInstance<PanelTextSettings>();AssetDatabase.CreateAsset(settings,settingsPath);}
            settings.defaultFontAsset=font;panel.textSettings=settings;EditorUtility.SetDirty(panel);EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();
            Debug.Log("Interface Vadronia preparada.");
        }
    }
}
