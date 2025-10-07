using UnityEngine;
using UnityEngine.Device;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate
            return;
        }

        Instance = this;
    }

    public string GetServerIP()
    {
        return PlayerPrefs.GetString("ServerIP");
    }
}
