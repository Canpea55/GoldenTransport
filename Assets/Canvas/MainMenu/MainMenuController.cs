using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public CanvasManager canvasManager;

    public VisualElement ui;

    public Button settingMenu;
    public Button transportations;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        settingMenu = ui.Q<Button>("SettingBtn");
        settingMenu.clicked += OnSettingsMenuClicked;

        transportations = ui.Q<Button>("MainBtn");
        transportations.clicked += OnTransportationClicked;
    }

    private void OnTransportationClicked()
    {
        StartCoroutine(canvasManager.SwitchScreen("transportations", 300));
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
