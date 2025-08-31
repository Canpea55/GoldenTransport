using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class SettingMenuController : MonoBehaviour
{
    public VisualElement ui;
    public VisualElement currentMenu;

    public VisualElement displaySettings;
    public VisualElement databaseSettings;

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
    }

    private void OnDisplayMenuClicked()
    {
        SetActiveSettings(displaySettings);
    }
    private void OnDatabaseMenuClicked()
    {
        SetActiveSettings(databaseSettings);
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
