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
    Label api;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void Start()
    {
        canvasManager = CanvasManager.Instance;
    }

    [System.Serializable]
    private class HandshakeResponse
    {
        public string version;
        public string message;
        public string datetime;
    }

    private void OnEnable()
    {
        settingMenu = ui.Q<Button>("SettingBtn");
        settingMenu.clicked += OnSettingsMenuClicked;

        transportations = ui.Q<Button>("MainBtn");
        transportations.clicked += OnTransportationClicked;

        version = ui.Q<Label>("Version");
        api = ui.Q<Label>("API");
        if(version != null) version.text = Application.version;
        if (api != null)
        {
            api.text = SettingsManager.Instance.GetServerIP();

            APIManager.Instance.Handshake(

                (jsonResponse) =>
                {
                    HandshakeResponse a = JsonUtility.FromJson<HandshakeResponse>(jsonResponse);
                    api.text = SettingsManager.Instance.GetServerIP() + $" ({a.version})";
                },

                // This is the onError callback
                (errorMessage) =>
                {
                    Debug.LogError("Handshake Failed: " + errorMessage);
                }
            );
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
