using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class MyAccountOrdersController : CanvasController, IOverlayWithSubmit
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

    private EventCallback<ChangeEvent<string>> docunoChanged;
    private EventCallback<ChangeEvent<string>> custnameChanged;
    private EventCallback<ChangeEvent<string>> remarkChanged;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }


    private void OnEnable()
    {
        close = ui.Q<Button>("Close"); close.clicked += () => Close();
        docuno = ui.Q<TextField>("Docuno");
        custname = ui.Q<TextField>("Custname");
        remark = ui.Q<TextField>("Remark");
    }

    private void Start()
    {
        cm = CanvasManager.Instance;
    }

    void Close()
    {
        StartCoroutine(cm.DisableOverlay(cm.currentOverlay.screenName, cm.currentOverlay.disablingDuration));
    }

    public override void OnCanvasLoaded()
    {
        base.OnCanvasLoaded();
        SetupValueChangedListeners();
    }

    public override void OnCanvasUnloaded()
    {
        base.OnCanvasUnloaded();
        RemoveValueChangedListeners();
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
                        ClearForm();
                        currentMode = Mode.Add;
                        submit.SetEnabled(false);
                        break;
                    case "edit":
                        submit.text = "บันทึก";
                        erease.style.display = DisplayStyle.Flex;
                        if (payload.TryGetValue("order", out object order))
                        {
                            var o = order as Order;
                            LoadOrder(o);
                            order_id = o.id;
                        }
                        submit.SetEnabled(false);
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

    private void SetupValueChangedListeners()
    {
        // Unregister existing (in case called multiple times)
        RemoveValueChangedListeners();

        // Define reusable callbacks
        docunoChanged = evt => EnableSubmit();
        custnameChanged = evt => EnableSubmit();
        remarkChanged = evt => EnableSubmit();

        // Register them
        docuno.RegisterValueChangedCallback(docunoChanged);
        custname.RegisterValueChangedCallback(custnameChanged);
        remark.RegisterValueChangedCallback(remarkChanged);
        Debug.Log(1);
    }

    private void RemoveValueChangedListeners()
    {
        if (docunoChanged != null)
            docuno.UnregisterValueChangedCallback(docunoChanged);

        if (custnameChanged != null)
            custname.UnregisterValueChangedCallback(custnameChanged);

        if (remarkChanged != null)
            remark.UnregisterValueChangedCallback(remarkChanged);
        Debug.Log(0);
    }

    private void EnableSubmit()
    {
        //// Optional: only enable when not empty
        //bool isReady = !string.IsNullOrEmpty(docuno.value?.Trim()) &&
        //               !string.IsNullOrEmpty(custname.value?.Trim());
        submit.SetEnabled(true);
    }

    public void LoadOrder(Order o)
    {
        // Set values without triggering callbacks
        docuno.SetValueWithoutNotify(o.docuno);
        custname.SetValueWithoutNotify(o.custname);
        remark.SetValueWithoutNotify(o.remark);

        // Re-enable logic if needed
        EnableSubmit();
    }

    public void ClearForm()
    {
        docuno.SetValueWithoutNotify("");
        custname.SetValueWithoutNotify("");
        remark.SetValueWithoutNotify("");
        submit.SetEnabled(false);
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
