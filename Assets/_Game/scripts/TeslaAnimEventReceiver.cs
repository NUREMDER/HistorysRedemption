using UnityEngine;

/// <summary>
/// Attach this script to the CHILD FBX model GameObject (the one with the Animator component)
/// inside the NikolaTesla hierarchy. Unity routes AnimationEvents to the same GameObject
/// where the Animator lives. Since TeslaAI is on the PARENT, the events can't reach it.
/// This script bridges that gap by catching AnimationEvents and forwarding them.
/// </summary>
public class TeslaAnimEventReceiver : MonoBehaviour
{
    private TeslaAI teslaAI;

    void Awake()
    {
        // Look for TeslaAI on the parent or any ancestor object
        teslaAI = GetComponentInParent<TeslaAI>();

        if (teslaAI == null)
        {
            Debug.LogWarning("TeslaAnimEventReceiver: Could not find TeslaAI on any parent object!");
        }
    }

    /// <summary>
    /// Called by AnimationEvents embedded in FBX animation clips (e.g., 'Martelo 2').
    /// Forwards the event to the parent TeslaAI script.
    /// </summary>
    public void TriggerAttackHit(int pointIndex)
    {
        // Event received and absorbed — TeslaAI handles hit detection via PerformHit coroutine,
        // so no action is needed here. This method exists solely to prevent
        // "AnimationEvent has no receiver" errors that break the Animator state machine.
    }
}
