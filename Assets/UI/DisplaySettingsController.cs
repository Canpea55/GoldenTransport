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
        vsync = ui.Query<Toggle>("VsyncToggle");
        framrateSelector = ui.Q<DropdownField>("FramerateSelector");
        if (vsync.value)
        {
            FrameLimiter.SetVSync(vsync.value);
        } else
        {
            SetFramerateLimitor(framrateSelector.value);
        }
        vsync.RegisterValueChangedCallback(evt => {
            FrameLimiter.SetVSync(vsync.value);
            if (vsync.value)
            {
                framrateSelector.SetEnabled(false);
            }
            else
            {
                framrateSelector.SetEnabled(true);
            }
        });
        framrateSelector.RegisterValueChangedCallback(evt =>
        {
            SetFramerateLimitor(framrateSelector.value);
        });
    }

    private void SetFramerateLimitor(string choice)
    {
        switch (choice)
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
    }
}
