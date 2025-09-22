using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class ShipmentFormController : CanvasController
{
    CanvasManager canvasManager;
    VisualElement ui;
    public Camera cam;

    public List<Order> orders = new List<Order>();

    public Button close;
    public Button addOrder;
    public Button submitTransportation;

    private IEnumerator LoadTransportationData()
    {
        StartCoroutine(CanvasManager.Instance.EnableOverlay("loading"));
        var date = ui.Q<TextField>("Date");
        date.value = DateTime.Today.ToString("yyyy-MM-dd");

        using (UnityWebRequest req = UnityWebRequest.Get("http://" + PlayerPrefs.GetString("ServerIP") + "/api/drivers"))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to fetch drivers: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            List<Driver> drivers = JsonUtilityWrapper.FromJsonList<Driver>(json);

            var driver = ui.Q<DropdownField>("Driver");
            driver.choices = drivers.Select(d => d.name).ToList();
            driver.index = 0;
        }

        using (UnityWebRequest req = UnityWebRequest.Get("http://" + PlayerPrefs.GetString("ServerIP") + "/api/vehicles"))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to fetch vehicles: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            List<Vehicles> veh = JsonUtilityWrapper.FromJsonList<Vehicles>(json);

            var vehicle = ui.Q<DropdownField>("Vehicle");
            vehicle.choices = veh.Select(d => d.name).ToList();
            vehicle.index = 0;
        }
        StartCoroutine(CanvasManager.Instance.DisableOverlay("loading", 600));
    }

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
        addOrder = ui.Q<Button>("AddOrder");
        close.clicked -= Close;
        close.clicked += Close;

        addOrder.clicked -= AddOrder;
        addOrder.clicked += AddOrder;
    }

    public override void OnReceiveData(object data)
    {
        Dictionary<string, object> payload = data as Dictionary<string, object>;
        payload.TryGetValue("type", out object uiType);
        if (ui != null)
        {
            var body = ui.Q<VisualElement>("Body");
            var itemsContainer = body.Q<VisualElement>("Items");
            itemsContainer.Clear();
            if (payload != null)
            {
                if (uiType == "add")
                {
                    addOrder.SetEnabled(true);
                    addOrder.style.display = DisplayStyle.Flex;
                    StartCoroutine(LoadTransportationData());
                }
                else
                {
                    addOrder.SetEnabled(false);
                    addOrder.style.display = DisplayStyle.None;
                }
            }
        }
    }

    public override void OnCanvasLoaded()
    {
        cam.enabled = true;
    }

    public override void OnCanvasUnloaded()
    {
        cam.enabled = false;
        if (ui != null)
        {
            var body = ui.Q<VisualElement>("Body");
            var itemsContainer = body.Q<VisualElement>("Items");
            itemsContainer.Clear();
        }
    }

    public void Close()
    {
        StartCoroutine(canvasManager.SwitchScreen(canvasManager.previousScreen.screenName, 300));
    }

    public void AddOrder()
    {
        var data = new Dictionary<string, object>
        {
            { "overlayType", "add" }
        };

        StartCoroutine(CanvasManager.Instance.EnableOverlay("orderDetails", data, (result) =>
        {
            var newOrder = result["newOrderData"];
            string docuno = (string)newOrder.GetType().GetProperty("docuno").GetValue(newOrder);
            string custname = (string)newOrder.GetType().GetProperty("custname").GetValue(newOrder);
            string remark = (string)newOrder.GetType().GetProperty("remark").GetValue(newOrder);

            Order a = new Order();
            a.docuno = docuno;
            a.custname = custname;
            a.remark = remark;

            orders.Add(a);
        }));
    }
}
