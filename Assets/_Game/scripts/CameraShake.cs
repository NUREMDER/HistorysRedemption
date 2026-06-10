using UnityEngine;
using Cinemachine;

/// <summary>
/// Singleton camera shake manager using Cinemachine Impulse system.
/// Attach to any GameObject in the scene. Automatically adds
/// CinemachineImpulseListener to all Virtual Cameras so they respond to impulses.
/// Usage: CameraShake.instance.Shake(0.3f, 0.1f);
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Get or add CinemachineImpulseSource component
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        // Auto-add CinemachineImpulseListener to all virtual cameras in the scene
        AutoAddImpulseListeners();
    }

    /// <summary>
    /// Finds all CinemachineVirtualCamera instances in the scene and ensures
    /// each one has a CinemachineImpulseListener extension attached.
    /// This removes the need to manually add the extension via Inspector.
    /// </summary>
    void AutoAddImpulseListeners()
    {
        CinemachineVirtualCamera[] vcams = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach (var vcam in vcams)
        {
            if (vcam.GetComponent<CinemachineImpulseListener>() == null)
            {
                vcam.gameObject.AddComponent<CinemachineImpulseListener>();
            }
        }
    }

    /// <summary>
    /// Triggers a camera shake impulse.
    /// </summary>
    /// <param name="intensity">How strong the shake is (0.1 = subtle, 0.5 = strong, 1.0 = extreme)</param>
    /// <param name="duration">How long the shake lasts in seconds</param>
    public void Shake(float intensity = 0.3f, float duration = 0.1f)
    {
        if (impulseSource == null) return;

        // Configure the impulse signal timing
        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;
        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = duration * 0.5f;

        // Generate the impulse with the specified intensity
        impulseSource.GenerateImpulse(intensity);
    }
}
