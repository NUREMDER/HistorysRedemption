using UnityEngine;

/// <summary>
/// Attach this script to the CHILD FBX model GameObject (the one with the Animator component)
/// inside the NikolaNewton hierarchy. Unity routes AnimationEvents to the same GameObject
/// where the Animator lives. Since NewtonAI is on the PARENT, the events can't reach it.
/// This script bridges that gap by catching AnimationEvents and forwarding them.
/// </summary>
public class NewtonAnimEventReceiver : MonoBehaviour
{
    private NewtonAI newtonAI;

    void Awake()
    {
        // Look for NewtonAI on the parent or any ancestor object
        newtonAI = GetComponentInParent<NewtonAI>();

        if (newtonAI == null)
        {
            Debug.LogWarning("NewtonAnimEventReceiver: Could not find NewtonAI on any parent object!");
        }
    }

    /// <summary>
    /// Called by AnimationEvents embedded in FBX animation clips (e.g., 'Martelo 2').
    /// Forwards the event to the parent NewtonAI script.
    /// </summary>
    public void TriggerAttackHit(int pointIndex)
    {
        // Event received and absorbed — NewtonAI handles hit detection via PerformHit coroutine,
        // so no action is needed here. This method exists solely to prevent
        // "AnimationEvent has no receiver" errors that break the Animator state machine.
    }
}
