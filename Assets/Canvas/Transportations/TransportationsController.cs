using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TransportationsController : CanvasController
{
    CanvasManager canvasManager;

    public VisualElement ui;

    public Button close;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void Start()
    {
        canvasManager = CanvasManager.Instance;
    }

    private void OnEnable()
    {
        close = ui.Q<Button>("Close");
        close.clicked += OnCloseClicked;

        //A Transport
        Button DEL11192568 = ui.Q<Button>(name: "DEL11192568");
        DEL11192568.clicked += () => {
            StartCoroutine(canvasManager.SwitchScreen("shipment", 900));
        };

        //A Order
        Button DEL11192568_1 = ui.Q<Button>(name: "DEL11192568_1");
        DEL11192568_1.clicked += () => {
            StartCoroutine(canvasManager.EnableOverlay("orderDetails"));
        };
    }

    private void OnCloseClicked()
    {
        StartCoroutine(canvasManager.SwitchScreen("main", 900));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
