using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class OrderDetailsController : CanvasController, IOverlayWithSubmit
{
    public VisualElement ui;
    CanvasManager cm;

    enum Mode
    {
        Add,
        Edit
    }

    public Button close;
    public Button submit;
    public Button erease;

    Dictionary<string, object> payload = null;

    private Mode currentMode = Mode.Add;
    private int order_id;
    private Action<Dictionary<string, object>> submitCallback; // <-- Store callback

    public TextField docuno;
    public TextField custname;
    public TextField remark;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        cm = CanvasManager.Instance;

        close = ui.Q<Button>("Close"); close.clicked += () => Close();
        docuno = ui.Q<TextField>("Docuno");
        custname = ui.Q<TextField>("Custname");
        remark = ui.Q<TextField>("Remark");
    }

    void Close()
    {
        StartCoroutine(cm.DisableOverlay("orderDetails", cm.currentOverlay.disablingDuration));
    }

    public override void OnReceiveData(object data)
    {
        payload = data as Dictionary<string, object>;

        if (ui != null && payload != null)
        {
            var root = ui;
            submit = root.Q<Button>("Submit");
            erease = root.Q<Button>("Erease");

            if (payload.TryGetValue("overlayType", out object overlayTypeObj))
            {
                string overlayType = overlayTypeObj as string;
                switch (overlayType)
                {
                    case "add":
                        submit.text = "เพิ่ม";
                        erease.style.display = DisplayStyle.None;
                        docuno.value = "";
                        custname.value = "";
                        remark.value = "";
                        currentMode = Mode.Add;
                        break;
                    case "edit":
                        submit.text = "บันทึก";
                        erease.style.display = DisplayStyle.Flex;
                        if (payload.TryGetValue("order", out object order))
                        {
                            var o = order as Order;
                            docuno.value = o.docuno;
                            custname.value = o.custname;
                            remark.value = o.remark;
                            order_id = o.id;
                        }
                        currentMode = Mode.Edit;
                        break;
                    default:
                        submit.text = "บันทึก";
                        submit.SetEnabled(false);
                        erease.style.display = DisplayStyle.Flex;
                        erease.SetEnabled(false);
                        break;
                }
            }

            // clear old listeners if needed
            submit.clicked -= Submit;
            submit.clicked += Submit;

            erease.clicked -= Erease;
            erease.clicked += Erease;
        }
    }

    public override void Submit()
    {
        if (payload == null)
        {
            Debug.LogWarning("No data received for this overlay.");
            return;
        }

        switch (currentMode)
        {
            case Mode.Add:
                payload["newOrderData"] = new
                {
                    docuno = docuno.value,
                    custname = custname.value,
                    remark = remark.value
                };
                break;
            case Mode.Edit:
                payload["orderData"] = new
                {
                    id = order_id,
                    docuno = docuno.value,
                    custname = custname.value,
                    remark = remark.value
                };
                break;
            default : 
                return;
        }

        submitCallback?.Invoke(payload);
        Close();
    }

    public void Erease()
    {
        if (payload == null)
        {
            Debug.LogWarning("No data received for this overlay.");
            return;
        }

        payload["orderData"] = new
        {
            id = order_id,
            delete = true
        };

        submitCallback?.Invoke(payload);
        Close();
    }

    public void SetSubmitCallback(Action<Dictionary<string, object>> callback)
    {
        submitCallback = callback;
    }
}
