using UnityEngine;

public class EndZoneTrigger : MonoBehaviour // Script adı sendekinden farklı olabilir
{
    private void OnTriggerEnter(Collider other)
    {
        // Giren objenin "Player" tagine sahip olduğundan emin oluyoruz
        if (other.CompareTag("Player"))
        {
            // Sahnedeki SceneChanger scriptini buluyoruz
            SceneChanger changer = FindObjectOfType<SceneChanger>();

            if (changer != null)
            {
                // Bir sonraki sahneye (Tutorial_Scene) gitmesini söylüyoruz
                changer.ChangeScene("Tutorial_Scene");
            }
            else
            {
                Debug.LogError("Sahnede SceneChanger objesi bulunamadı!");
            }
        }
    }
}