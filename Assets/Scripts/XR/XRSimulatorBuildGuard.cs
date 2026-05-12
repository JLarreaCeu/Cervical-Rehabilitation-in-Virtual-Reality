// Destroys the XR Interaction Simulator on every scene load in non-editor builds.
// The simulator feeds static/default device poses which override TrackedPoseDriver,
// locking gaze to center on device.
//
// Uses DestroyImmediate so the GO is gone before any Start() or Update() runs.
// The OnSceneLoaded path handles every scene after the first one.

#if !UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

public static class XRSimulatorBuildGuard
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        KillXRSimulator();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        KillXRSimulator();
    }

    static void KillXRSimulator()
    {
        var sim = GameObject.Find("XR Interaction Simulator");
        if (sim != null)
        {
            Object.DestroyImmediate(sim);
            Debug.Log("[XRSimulatorBuildGuard] Killed XR Interaction Simulator in: "
                      + SceneManager.GetActiveScene().name);
        }
    }
}
#endif
