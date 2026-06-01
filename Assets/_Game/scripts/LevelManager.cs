using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject invisibleWall; // Drag and drop the ParkourBlocker here
    public float delay = 1.0f;       // Time delay before locking the wall

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Activate the wall after the specified delay once the player passes
            Invoke("ActivateWall", delay);

            // Immediately turn off parkour mode to start the combat
            var parkur = other.GetComponent<ParkourController2d>();
            if (parkur != null)
            {
                parkur.isParkourActive = false;
                
                // Reset player velocity to make them stop moving
                other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }

            Debug.Log("Line crossed, wall will close in 1 second!");
        }
    }

    void ActivateWall()
    {
        if (invisibleWall != null)
        {
            // Lock the path behind the player
            invisibleWall.SetActive(true);
            Debug.Log("WALL ACTIVATED! No turning back.");
        }
        
        // Destroy this trigger object since it is no longer needed
        Destroy(gameObject);
    }
}