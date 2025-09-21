using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OrderDetailsController : CanvasController, IOverlayWithSubmit
{
    public VisualElement ui;

    public Button close;
    public Button submit;
    public Button erease;

    Dictionary<string, object> payload = null;

    private Action<Dictionary<string, object>> submitCallback; // <-- Store callback


    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        close = ui.Q<Button>("Close");
        erease = ui.Q<Button>("Erease");
        close.clicked += () =>
        {
            StartCoroutine(CanvasManager.Instance.DisableOverlay("orderDetails", 600));
        };
    }

    public override void OnReceiveData(object data)
    {
        payload = data as Dictionary<string, object>;

        if (ui != null && payload != null)
        {
            var root = ui;
            submit = root.Q<Button>("Submit");

            if (payload.TryGetValue("overlayType", out object overlayTypeObj))
            {
                string overlayType = overlayTypeObj as string;
                switch (overlayType)
                {
                    case "add":
                        submit.text = "เพิ่ม";
                        erease.style.display = DisplayStyle.None;
                        break;
                    default:
                        submit.text = "บันทึก";
                        erease.style.display = DisplayStyle.Flex;
                        break;
                }
            }

            // clear old listeners if needed
            submit.clicked -= OnSubmit;
            submit.clicked += OnSubmit;
        }
    }

    public void OnSubmit()
    {
        if (payload == null)
        {
            Debug.LogWarning("No data received for this overlay.");
            return;
        }

        // Example: Add new field to send back
        payload["newOrderData"] = new { id = 123, name = "Order A" };

        // Call callback to send data back to ShipmentFormController
        submitCallback?.Invoke(payload);

        StartCoroutine(CanvasManager.Instance.DisableOverlay("orderDetails", 600));
    }

    public void SetSubmitCallback(Action<Dictionary<string, object>> callback)
    {
        submitCallback = callback;
    }
}
