using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

public class DisplaySettingsController : MonoBehaviour
{
    public VisualElement ui;

    public DropdownField framrateSelector;
    public Toggle vsync;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        framrateSelector = ui.Q<DropdownField>("FramerateSelector");
        if (PlayerPrefs.HasKey("FramerateLimit"))
        {
            framrateSelector.value = PlayerPrefs.GetString("FramerateLimit");
        }
        else
        {
            SetFramerateLimitor("60");
        }

        vsync = ui.Query<Toggle>("VsyncToggle");
        if (PlayerPrefs.HasKey("VSync"))
        {
            vsync.value = PlayerPrefs.GetInt("VSync") != 0;
            SetVSync(vsync.value, framrateSelector);
        } 
        else
        {
            vsync.value = true;
            SetVSync(vsync.value, framrateSelector);
        }

        vsync.RegisterValueChangedCallback(evt => {
            SetVSync(vsync.value, framrateSelector);
        });

        framrateSelector.RegisterValueChangedCallback(evt =>
        {
            SetFramerateLimitor(framrateSelector.value);
        });
    }

    private void SetFramerateLimitor(string value)
    {
        switch (value)
        {
            default:
                FrameLimiter.SetTarget(60);
                break;
            case "24":
                FrameLimiter.SetTarget(24);
                break;
            case "30":
                FrameLimiter.SetTarget(30);
                break;
            case "60":
                FrameLimiter.SetTarget(60);
                break;
            case "90":
                FrameLimiter.SetTarget(90);
                break;
            case "120":
                FrameLimiter.SetTarget(120);
                break;
            case "144":
                FrameLimiter.SetTarget(144);
                break;
            case "165":
                FrameLimiter.SetTarget(165);
                break;
            case "Unlimited":
                FrameLimiter.SetTarget(-1);
                break;
        }
        PlayerPrefs.SetString("FramerateLimit", value);
    }

    private void SetVSync(bool value, DropdownField frameselecetor)
    {
        FrameLimiter.SetVSync(value);
        PlayerPrefs.SetInt("VSync", value ? 1 : 0);
        if (value)
        {
            framrateSelector.SetEnabled(false);
        }
        else
        {
            framrateSelector.SetEnabled(true);
        }
    }
}
