using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vadronia.Editor
{
    [InitializeOnLoad]
    public static class DemoSetup
    {
        const string ScenePath = "Assets/Scenes/Vadronia.unity";
        static DemoSetup() { EditorApplication.delayCall += FirstOpen; }
        static void FirstOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (!System.IO.File.Exists(ScenePath)) CreateScene();
        }
        [MenuItem("Vadronia/Abrir demo")]
        public static void OpenDemo()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!System.IO.File.Exists(ScenePath)) CreateScene();
            else EditorSceneManager.OpenScene(ScenePath);
        }
        static void CreateScene()
        {
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Vadronia/characters-original.png");
            if (atlas == null) { Debug.LogError("Atlas Assets/Resources/Vadronia/characters-original.png ausente."); return; }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Vadronia Demo").AddComponent<VadroniaDemo>().characterAtlas = atlas;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.companyName = "Projeto R";
            PlayerSettings.productName = "Vadronia — Grünwald";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Vadronia/Verificar demo")]
        public static void ValidateDemo()
        {
            int count = AdventureChecks.Run(message => Debug.Log(message));
            foreach (string name in new[] { "terrain-v2", "terrain-v3", "town", "town-extra", "characters-original", "player-video/south", "player-video/southeast", "player-video/east", "player-video/northeast", "player-video/north", "player-video/northwest", "player-video/west" })
            {
                var texture = Resources.Load<Texture2D>("Vadronia/" + name);
                if (texture == null) throw new System.Exception("Textura ausente: " + name);
                if (texture.filterMode != FilterMode.Point) throw new System.Exception("Filtro incorreto: " + name);
            }
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                throw new System.Exception("Esta demo usa Built-in. Remova o pipeline customizado antes de testar.");
            Debug.Log(count + " verificações de lógica passaram; atlas e pipeline verificados. Teste também em Play.");
        }
        [MenuItem("Vadronia/Gerar executável Windows")]
        public static void BuildWindows()
        {
            if (!System.IO.File.Exists(ScenePath)) CreateScene();
            string folder = EditorUtility.OpenFolderPanel("Pasta do executável", "", "");
            if (string.IsNullOrEmpty(folder)) return;
            var report = BuildPipeline.BuildPlayer(new[] { ScenePath },
                System.IO.Path.Combine(folder, "Vadronia.exe"), BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Build falhou. Consulte o Console da Unity.");
        }
    }
    public sealed class PixelImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/Vadronia/")) return;
            var t = (TextureImporter)assetImporter;
            t.textureType = TextureImporterType.Default;
            t.alphaSource = TextureImporterAlphaSource.FromInput;
            t.alphaIsTransparency = true;
            t.filterMode = FilterMode.Point;
            t.mipmapEnabled = false;
            t.textureCompression = TextureImporterCompression.Uncompressed;
            t.npotScale = TextureImporterNPOTScale.None;
            t.maxTextureSize = 2048;
        }
    }
}
