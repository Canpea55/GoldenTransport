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
        Debug.Log("start setting menu");
        StartCoroutine(canvasManager.EnableScreen("settings"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
