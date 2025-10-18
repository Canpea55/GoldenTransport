using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : CanvasController
{
    CanvasManager canvasManager;

    public VisualElement ui;

    public Button settingMenu;
    public Button transportations;

    Label version;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void Start()
    {
        canvasManager = CanvasManager.Instance;
    }

    private void OnEnable()
    {
        settingMenu = ui.Q<Button>("SettingBtn");
        settingMenu.clicked += OnSettingsMenuClicked;

        transportations = ui.Q<Button>("MainBtn");
        transportations.clicked += OnTransportationClicked;

        version = ui.Q<Label>("Version");
        if(version != null)
        {
            version.text = Application.version;
        }
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
