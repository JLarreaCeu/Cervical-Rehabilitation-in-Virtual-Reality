using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    static MonoBehaviour _runner;

    static MonoBehaviour Runner()
    {
        if (_runner != null) return _runner;
        var go = new GameObject("SceneLoaderRunner");
        Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<SceneLoaderRunner>();
        return _runner;
    }

    public static void Load(string sceneName)
    {
        Runner().StartCoroutine(LoadAsync(sceneName));
    }

    static IEnumerator LoadAsync(string sceneName)
    {
        yield return null;
        yield return SceneManager.LoadSceneAsync(sceneName);
    }

    class SceneLoaderRunner : MonoBehaviour { }
}
