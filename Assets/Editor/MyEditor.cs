using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class MyEditor
{
    [MenuItem("Text/Convert \\r\\n")]
    public static void Convert()
    {
        var filePath = "Assets/Stages/Stage_1.txt";
        var file = File.ReadAllLines(filePath);
        var convert = "";
        for(int i = 0; i < file.Length; i ++)
        {
            convert += file[i];
            convert += '\n';
        }
        File.WriteAllText(filePath, convert);
    }

#if UNITY_ANDROID

    [MenuItem("Android/Build Android Project")]
    public static void BuildAndroidProject()
    {

        var buildPathes = new string[EditorBuildSettings.scenes.Length];
        for (var i = 0; i < buildPathes.Length; i++)
        {
            buildPathes[i] = EditorBuildSettings.scenes[i].path;
        }
#if UNITY_EDITOR_OSX
        var projectPath = "/Users/haobo/Unity_Android/Snake3D_fresh";
#else
    var projectPath = "D:/Workspace/Snake3D_Android/fresh";
#endif
        BuildPipeline.BuildPlayer(buildPathes, projectPath,
                BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
    }
    
    [MenuItem("Android/Update Build Code")]
    public static void UpdateBuildCode()
    {
        var code = PlayerSettings.Android.bundleVersionCode;
        PlayerSettings.Android.bundleVersionCode++;
        Debug.Log("Update build code updated from " + code + " to " + PlayerSettings.Android.bundleVersionCode);
    }

#endif

#if UNITY_IOS

    [MenuItem("iOS Project/Build iOS Project")]
    public static void BuildiOSProject()
    {
        var buildPathes = new string[EditorBuildSettings.scenes.Length];
        for (var i = 0; i < buildPathes.Length; i++)
        {
            buildPathes[i] = EditorBuildSettings.scenes[i].path;
        }
        BuildPipeline.BuildPlayer(buildPathes, "Users/haobo/",
                BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer);
    }
    
    [MenuItem("iOS Project/Update Build Code")]
    public static void UpdateBuildCode()
    {
        var code = PlayerSettings.iOS.buildNumber;
        PlayerSettings.iOS.buildNumber = (int.Parse(code) + 1).ToString();
        Debug.Log("Update build code updated from " + code + " to " + PlayerSettings.iOS.buildNumber);
    }

#endif

    [MenuItem("Scene/Replace Font")]
    public static void ReplaceFont()
    {
        var font = (Font)Selection.objects[0];

        Debug.Log("Replace Font with: " + font.ToString());

        var yourLabels = GameObject.FindObjectsOfType<Text>();
        foreach (Text someLabel in yourLabels)
        {
            someLabel.font = font;
        }

    }

    [MenuItem("GameObject/Apply All Prefabs")]
    public static void ApplyAllPrefabs()
    {
        var allPrefabs = Selection.gameObjects;
        foreach (var prefab in allPrefabs)
        {
            Selection.activeGameObject = prefab;
            EditorApplication.ExecuteMenuItem("GameObject/Apply Changes To Prefab");
            Debug.Log("Apply changes to " + prefab.name);
        }
    }

    [MenuItem("GameObject/Revert All Prefabs")]
    public static void RevertAllPrefabs()
    {
        var allPrefabs = Selection.gameObjects;
        foreach (var prefab in allPrefabs)
        {
            PrefabUtility.RevertPrefabInstance(prefab);
            Debug.Log("Changes reverted" + prefab.name);
        }
    }
}
