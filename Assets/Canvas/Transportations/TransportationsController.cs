using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TransportationsController : CanvasController
{
    public VisualElement ui;

    public Button close;
    public Button addShipment;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        close = ui.Q<Button>("Close");
        close.clicked += OnCloseClicked;

        addShipment = ui.Q<Button>("AddShipment");
        addShipment.clicked += () => {
            StartCoroutine(CanvasManager.Instance.SwitchScreen("shipmentForm", 900));
        };

        //A Transport
        Button DEL11192568 = ui.Q<Button>(name: "DEL11192568");
        DEL11192568.clicked += () => {
            StartCoroutine(CanvasManager.Instance.SwitchScreen("shipment", 900));
        };

        //A Order
        Button DEL11192568_1 = ui.Q<Button>(name: "DEL11192568_1");
        DEL11192568_1.clicked += () => {
            StartCoroutine(CanvasManager.Instance.EnableOverlay("orderDetails"));
        };
    }

    private void OnCloseClicked()
    {
        StartCoroutine(CanvasManager.Instance.SwitchScreen("main", 900));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
