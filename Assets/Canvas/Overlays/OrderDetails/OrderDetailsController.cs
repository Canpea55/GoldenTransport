using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OrderDetailsController : CanvasController
{
    public VisualElement ui;

    public Button close;
    public Button submit;
    public Button erease;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        close = ui.Q<Button>("Close");
        close.clicked += () =>
        {
            StartCoroutine(CanvasManager.Instance.DisableOverlay("orderDetails", 600));
        };
    }

    public override void OnReceiveData(object data)
    {
        Dictionary<string, object> payload = data as Dictionary<string, object>;

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
                        break;
                    default:
                        submit.text = "บันทึก";
                        break;
                }
            }

            //// clear old listeners if needed
            //submit.clicked -= OnSubmitClicked;
            //submit.clicked += () =>
            //{
            //    if (payload == null)
            //    {
            //        Debug.LogWarning("No data received for this overlay.");
            //        return;
            //    }

            //    // Now read values safely
            //    if (payload.TryGetValue("overlayType", out var overlayType))
            //    {
            //        Debug.Log("Overlay Type: " + overlayType);
            //    }

            //    if (payload.TryGetValue("other", out var otherValue))
            //    {
            //        Debug.Log("Other Value: " + otherValue);
            //    }

            //    // Example: send to backend or close overlay here
            //};
        }
    }
}
