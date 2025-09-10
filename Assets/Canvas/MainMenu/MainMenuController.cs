using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public CanvasManager canvasManager;

    public VisualElement ui;

    public Button settingMenu;
    public Button transportation;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        settingMenu = ui.Q<Button>("SettingBtn");
        settingMenu.clicked += OnSettingsMenuClicked;
        
        transportation = ui.Q<Button>("MainBtn");
        transportation.clicked += OnTransportationClicked;
    }

    private void OnTransportationClicked()
    {
        StartCoroutine(canvasManager.SwitchScreen("transportation", 300));
    }

    private void OnSettingsMenuClicked()
    {
        StartCoroutine(canvasManager.SwitchScreen("settings", 300));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
