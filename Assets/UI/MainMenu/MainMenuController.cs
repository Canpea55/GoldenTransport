using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public CanvasManager canvasManager;

    public VisualElement ui;

    public Button settingMenu;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        settingMenu = ui.Q<Button>("SettingBtn");
        settingMenu.clicked += OnSettingsMenuClicked;
    }

    private void OnSettingsMenuClicked()
    {
        StartCoroutine(canvasManager.EnableScreen("settings"));
        StartCoroutine(canvasManager.DisableScreen("main", 300));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
