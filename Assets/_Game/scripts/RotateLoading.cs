using UnityEngine;

public class RotateLoading : MonoBehaviour
{
    public float rotateSpeed = 800f; 
    private RectTransform rectComponent;

    void Start()
    {
        rectComponent = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rectComponent != null)
        {
            rectComponent.Rotate(new Vector3(0, 0, -rotateSpeed * Time.unscaledDeltaTime));
        }
    }
}