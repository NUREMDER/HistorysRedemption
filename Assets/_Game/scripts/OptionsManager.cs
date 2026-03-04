using UnityEngine;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown;

    void Start()
    {
        if (difficultyDropdown != null)
        {
            difficultyDropdown.value = PlayerPrefs.GetInt("GameDifficulty", 0);
        }
    }

    public void SetDifficulty(int index)
    {
        PlayerPrefs.SetInt("GameDifficulty", index);
        PlayerPrefs.Save();
        Debug.Log("DIFFICULTY SET TO: " + index);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("ALL SAVE DATA RESET!");
    }
}