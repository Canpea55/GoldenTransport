using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TransportUIController : MonoBehaviour
{
    public CanvasManager canvasManager;

    public VisualElement ui;

    public Button close;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        close = ui.Q<Button>("Close");
        close.clicked += OnCloseClicked;
    }

    private void OnCloseClicked()
    {
        //StartCoroutine(canvasManager.DisableScreen("transportation", 1000));
        //StartCoroutine(canvasManager.EnableScreen("main"));
        StartCoroutine(canvasManager.SwitchScreen("main", 900));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
