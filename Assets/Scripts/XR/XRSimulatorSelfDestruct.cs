// Attached to the XR Interaction Simulator GameObject in scenes that need it for editor
// testing but must not run it in device builds.
//
// Script Execution Order is set to -32000 so this Awake() fires BEFORE any simulator
// component (XRInteractionSimulator, SimulatedDeviceLifecycleManager, etc.) can run,
// preventing the simulated HMD device from ever being created.

#if !UNITY_EDITOR
using UnityEngine;

public class XRSimulatorSelfDestruct : MonoBehaviour
{
    void Awake()
    {
        DestroyImmediate(gameObject);
    }
}
#else
using UnityEngine;
public class XRSimulatorSelfDestruct : MonoBehaviour { }
#endif
