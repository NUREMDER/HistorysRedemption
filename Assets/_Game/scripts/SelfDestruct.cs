using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    // Baþlar baþlamaz geri sayýmý baþlat
    void Start()
    {
        // 1 saniye sonra bu objeyi oyundan sil
        Destroy(gameObject, 1.0f);
    }
}