using UnityEngine;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityToolbarExtender;
#endif
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
[InitializeOnLoad]
public class SceneSwitchButton
{
    static SceneSwitchButton()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
    }

    private static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            fixedHeight = 25,
            fixedWidth = 100
        };


        if (GUILayout.Button(new GUIContent("Loading")))
        {
            SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/Loading.unity");
        }

        if (GUILayout.Button(new GUIContent("Gameplay")))
        {
            SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/Gameplay.unity");
        }
         if (GUILayout.Button(new GUIContent("Test")))
        {
            SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/Test.unity");
        }

    }
}
#endif

public static class SceneHelper
{
#if UNITY_EDITOR
    public static void StartScene(string scenePath, bool play = false)
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }

        EditorSceneManager.OpenScene(scenePath);
        EditorApplication.isPlaying = play;
    }

    public static void StartSceneWithSavePrompt(string scenePath, bool play = false)
    {
        // Check if there are unsaved changes in the current scene
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // User chose to save (or no changes) - load the new scene
            StartScene(scenePath, play);
        }
        else
        {
            // User canceled the save operation - do not load the scene
            Debug.Log("Scene load canceled because user didn't save changes.");
        }
    }
#endif
}