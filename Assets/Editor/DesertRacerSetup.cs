using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class DesertRacerSetup
{
    static DesertRacerSetup(){ EditorApplication.delayCall += CreateSceneIfNeeded; }
    static void CreateSceneIfNeeded(){
        const string scenePath="Assets/Scenes/DesertRally.unity";
        if(File.Exists(scenePath)) return;
        if(!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var go=new GameObject("DESERT RALLY GAME"); go.AddComponent<DesertRacerGame>();
        EditorSceneManager.SaveScene(scene,scenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
        PlayerSettings.activeInputHandler = 0;
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("Desert Rally scene created. Press Play to race!");
    }
}
