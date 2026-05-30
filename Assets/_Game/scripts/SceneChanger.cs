using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    [Header("Visual Settings")]
    public GameObject loadingCanvas; // The Canvas that contains the spinning loading icon
    public float waitTime = 3f;      // How many seconds the loading screen will stay visible

    // Call this function when the parkour ends or when a UI button is pressed
    public void ChangeScene(string targetSceneName)
    {
        StartCoroutine(LoadingProcess(targetSceneName));
    }

    IEnumerator LoadingProcess(string sceneName)
    {
        //  Open the loading screen (the spinning icon starts rotating automatically)
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(true);
        }

        // Wait for the specified duration 
        yield return new WaitForSeconds(waitTime);

        //  Load the target scene safely
        SceneManager.LoadScene(sceneName);
    }
}