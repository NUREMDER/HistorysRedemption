using UnityEngine;

public class RotateLoading : MonoBehaviour
{
    public float rotateSpeed = 800f; // Rotation speed of the loading icon
    private RectTransform rectComponent;

    void Start()
    {
        // Get the RectTransform component since this script is used on a UI element
        rectComponent = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rectComponent != null)
        {
            // Rotate the UI element clockwise on the Z axis
            // unscaledDeltaTime ensures the icon keeps spinning even if the game is paused (timeScale = 0)
            rectComponent.Rotate(new Vector3(0, 0, -rotateSpeed * Time.unscaledDeltaTime));
        }
    }
}