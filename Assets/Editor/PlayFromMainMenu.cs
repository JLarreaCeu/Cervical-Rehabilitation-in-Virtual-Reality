using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Always start Play mode from the main menu scene (MainMenu),
/// regardless of which scene is currently open in the editor.
/// </summary>
[InitializeOnLoad]
public static class PlayFromMainMenu
{
    const string MENU_SCENE = "Assets/Scenes/MainMenu.unity";
    const string MENU_ITEM  = "Tools/Play From Main Menu";

    static string _previousScene;

    static PlayFromMainMenu()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem(MENU_ITEM)]
    static void ToggleSetting()
    {
        bool current = EditorPrefs.GetBool(MENU_ITEM, true);
        EditorPrefs.SetBool(MENU_ITEM, !current);
        Menu.SetChecked(MENU_ITEM, !current);
    }

    [MenuItem(MENU_ITEM, true)]
    static bool ToggleSettingValidate()
    {
        bool enabled = EditorPrefs.GetBool(MENU_ITEM, true);
        Menu.SetChecked(MENU_ITEM, enabled);
        return true;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        bool enabled = EditorPrefs.GetBool(MENU_ITEM, true);
        if (!enabled) return;

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Save current scene and switch to menu
            _previousScene = EditorSceneManager.GetActiveScene().path;
            if (_previousScene != MENU_SCENE)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(MENU_SCENE);
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Restore the scene we were editing
            if (!string.IsNullOrEmpty(_previousScene) && _previousScene != MENU_SCENE)
            {
                EditorSceneManager.OpenScene(_previousScene);
                _previousScene = null;
            }
        }
    }
}