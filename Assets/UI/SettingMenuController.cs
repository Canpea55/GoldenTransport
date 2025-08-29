using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class SettingMenuController : MonoBehaviour
{
    public VisualElement ui;

    public Button displayMenuBtn;
    public Button databaseMenuBtn;
    public Button stickerManageMenuBtn;
    public Button vehManageMenuBtn;
    public Button driverManageMenuBtn;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        displayMenuBtn = ui.Q<Button>("Display");
        displayMenuBtn.clicked += OnDisplayMenuClicked;
    }

    private void OnDisplayMenuClicked()
    {
        Debug.Log("Display!");
    }
}
