using UnityEngine;

public static class SaveManager
{
    // PlayerPrefs is a Unity library that can save and load data from a dictionary the persists between plays,
    // essentially like saving a file on the computer
    
    private const string PREFS_SAVE_KEY = "Save";    
    
    // Saves a savable board object into PlayerPrefs
    public static void Save(SavableBoard sb)
    {
        string json = JsonUtility.ToJson(sb);
        PlayerPrefs.SetString(PREFS_SAVE_KEY, json);
    }

    // Returns a savable board object from PlayerPrefs if exists, else returns null
    public static SavableBoard Load()
    {
        if (PlayerPrefs.HasKey(PREFS_SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(PREFS_SAVE_KEY);
            return JsonUtility.FromJson<SavableBoard>(json);
        }

        return null;
    }
}
