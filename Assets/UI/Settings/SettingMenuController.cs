using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class SettingMenuController : MonoBehaviour
{
    public CanvasManager canvasManager;

    public VisualElement ui;
    public VisualElement currentMenu;

    public VisualElement displaySettings;
    public VisualElement databaseSettings;
    public VisualElement stickersManager;

    public Button close;
    public Button displayMenuBtn;
    public Button databaseMenuBtn;
    public Button stickerManageMenuBtn;
    public Button vehManageMenuBtn;
    public Button driverManageMenuBtn;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;

        List<VisualElement> settingsList = new List<VisualElement>();

        displaySettings = ui.Q<VisualElement>("DisplaySettings");
        settingsList.Add(displaySettings);
        databaseSettings = ui.Q<VisualElement>("DatabaseSettings");
        settingsList.Add(databaseSettings);
        stickersManager = ui.Q<VisualElement>("StickersManager");
        settingsList.Add(stickersManager);

        //Auto hide other tab then the display tab (first setting tab)
        for(int i = 0; i <  settingsList.Count; i++)
        {
            settingsList[i].style.display = DisplayStyle.None;
            settingsList[i].SetEnabled(false);
        }

        SetActiveSettings(displaySettings);
    }

    private void OnEnable()
    {
        displayMenuBtn = ui.Q<Button>("Display");
        displayMenuBtn.clicked += OnDisplayMenuClicked;

        databaseMenuBtn = ui.Q<Button>("Database");
        databaseMenuBtn.clicked += OnDatabaseMenuClicked;

        stickerManageMenuBtn = ui.Q<Button>("Stickers");
        stickerManageMenuBtn.clicked += OnStickerMenuClicked;

        close = ui.Q<Button>("CloseSettingMenu");
        close.clicked += OnCloseButtonClicked;
    }

    private void OnCloseButtonClicked()
    {
        //temporary
        StartCoroutine(canvasManager.DisableScreen("settings", 300));
    }

    private void OnDisplayMenuClicked()
    {
        SetActiveSettings(displaySettings);
    }
    private void OnDatabaseMenuClicked()
    {
        SetActiveSettings(databaseSettings);
    }
    private void OnStickerMenuClicked()
    {
        StickerTable stkTable = GetComponent<StickerTable>();
        if (stkTable != null)
        {
            stkTable.StartTable();
            SetActiveSettings(stickersManager);
        }
    }

    public void SetActiveSettings(VisualElement toSettings)
    {
        if (currentMenu == null)
        {
            currentMenu = toSettings;
            currentMenu.style.display = DisplayStyle.Flex;
            toSettings.SetEnabled(true);
        }
        else
        {
            if(currentMenu != toSettings)
            {
                currentMenu.SetEnabled(false);
                currentMenu.schedule.Execute(() =>
                {
                    currentMenu.style.display = DisplayStyle.None;
                    currentMenu = toSettings;
                    currentMenu.style.display = DisplayStyle.Flex;
                    currentMenu.SetEnabled(true);
                }).StartingIn(250);
            }
        }
    }
}
