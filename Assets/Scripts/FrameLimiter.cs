using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
    public static int targetFPS = 60; // set to -1 for unlimited
    public static bool vsync = false;

    void Awake()
    {
        SetVSync(false);
        Application.targetFrameRate = targetFPS;
    }

    public static void SetTarget(int target)
    {
        SetVSync(false);
        if (!(target == targetFPS)) {
            targetFPS = target;
            Application.targetFrameRate = targetFPS;
        }
    }

    public static void SetVSync(bool state)
    {
        if (state) {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        } else {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFPS;
        }
    }
}
