using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    void Start()
    {
        // Automatically destroy this object 1 second after it spawns to save memory
        Destroy(gameObject, 1.0f);
    }
}