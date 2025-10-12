using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class ShipmentFormController : CanvasController
{
    CanvasManager cm;
    VisualElement ui;
    public Camera cam;

    public SettingController settingController;

    public List<Vehicles> vehicles = new List<Vehicles>();
    public List<Driver> drivers = new List<Driver>();
    public List<Order> orders = new List<Order>();

    public Button close;
    public Button addOrder;
    public Button loadOrders;
    public Button submitShipment;

    public TextField date;
    public DropdownField vehicle;
    public DropdownField driver;

    private IEnumerator LoadTransportationData()
    {
        StartCoroutine(CanvasManager.Instance.EnableOverlay("loading"));
        date = ui.Q<TextField>("Date");
        date.value = DateTime.Today.ToString("yyyy-MM-dd");

        using (UnityWebRequest req = UnityWebRequest.Get("http://" + settingController.getServerIP() + "/api/drivers"))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to fetch drivers: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            drivers = JsonUtilityWrapper.FromJsonList<Driver>(json);

            driver = ui.Q<DropdownField>("Driver");
            driver.choices = drivers.Select(d => d.name).ToList();
            driver.index = 0;
        }

        using (UnityWebRequest req = UnityWebRequest.Get("http://" + settingController.getServerIP() + "/api/vehicles"))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to fetch vehicles: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            vehicles = JsonUtilityWrapper.FromJsonList<Vehicles>(json);

            vehicle = ui.Q<DropdownField>("Vehicle");
            vehicle.choices = vehicles.Select(d => d.name).ToList();
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
        cm = CanvasManager.Instance;
    }

    private void OnEnable()
    {
        close = ui.Q<Button>("Close");
        addOrder = ui.Q<Button>("AddOrder");
        submitShipment = ui.Q<Button>("Submit");
        loadOrders = ui.Q<Button>("Load");

        close.clicked += Close;
        addOrder.clicked += AddOrder;
        submitShipment.clicked += Submit;
        loadOrders.clicked += LoadOrders;
    }

    void LoadOrders()
    {
        StartCoroutine(CanvasManager.Instance.EnableOverlay("myAccountOrders", null, (result) =>
        {
            Debug.Log(result);
            //var newOrder = result["newOrderData"];
            //string docuno = (string)newOrder.GetType().GetProperty("docuno").GetValue(newOrder);
            //string custname = (string)newOrder.GetType().GetProperty("custname").GetValue(newOrder);
            //string remark = (string)newOrder.GetType().GetProperty("remark").GetValue(newOrder);

            //Order a = new Order();
            //a.docuno = docuno;
            //a.custname = custname;
            //a.remark = remark;

            //orders.Add(a);
            //UpdateUI();
        }));
    }

    public override void Submit()
    {
        base.Submit();
        //shipment.remark = remark.value;
        Shipment shipment = new Shipment();
        shipment.vehicle_id = vehicles[vehicle.index].id;
        shipment.driver_id = drivers[driver.index].id;
        shipment.delivery_date = date.value;
        shipment.orders = orders;
        
        StartCoroutine(PostShipment(shipment));
        StartCoroutine(cm.SwitchScreen(cm.previousScreen.screenName, cm.currentScreen.disablingDuration));
    }

    [Serializable]
    class NewShipment { 
        public string remark; 
        public int vehicle_id; 
        public int driver_id; 
        public string delivery_date; 
        public List<NewOrder> orders = new List<NewOrder>(); 
    }

    [Serializable]
    class NewOrder { 
        public string docuno; 
        public string custname; 
        public string remark; 
        public string status; 
        public NewOrder(string docno, string custn, string rema, string stat) 
        { 
            docuno = docno; 
            custname = custn; 
            remark = rema; 
            status = stat; 
        } 
    }

    IEnumerator PostShipment(Shipment shipment)
    {
        NewShipment s = new NewShipment(); 
        s.remark = shipment.remark; 
        s.vehicle_id = shipment.vehicle_id; 
        s.driver_id = shipment.driver_id; 
        s.delivery_date = shipment.delivery_date; 
        s.orders = new List<NewOrder>(); 
        foreach (var or in shipment.orders) 
        { 
            s.orders.Add(new NewOrder(or.docuno, or.custname, or.remark, or.status)); 
        }
        
        using (UnityWebRequest req = new UnityWebRequest("http://" + SettingsManager.Instance.GetServerIP() + "/api/shipment", "POST"))
        {
            string json = JsonUtility.ToJson(s);
            Debug.Log(json);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error posting shipment: " + req.error + " - " + req.downloadHandler.text);
            }
            else
            {
                Debug.Log("Shipment posted successfully: " + req.downloadHandler.text);
            }
        }
    }

    public override void OnReceiveData(object data)
    {
        Dictionary<string, object> payload = data as Dictionary<string, object>;
        payload.TryGetValue("type", out object uiType);
        if (ui != null)
        {
            var itemsContainer = ui.Q<VisualElement>("Items");
            itemsContainer.Clear();
            if (payload != null)
            {
                if (uiType == "add")
                {
                    orders.Clear();
                    UpdateUI();
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
    }

    public void Close()
    {
        StartCoroutine(cm.SwitchScreen(cm.previousScreen.screenName, cm.currentScreen.disablingDuration));
        orders.Clear();
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
            UpdateUI();
        }));
    }

    public void RemoveOrder(Order item)
    {
        orders.Remove(item);
        UpdateUI();
    }

    public void UpdateUI()
    {
        var items = ui.Q<VisualElement>("Items");
        if (items != null)
        {
            items.Clear();

            if (orders.Count > 0)
            {
                submitShipment.SetEnabled(true);

                var counter = 1;
                foreach (var order in orders)
                {
                    // --- Main Item Container ---
                    var item = new VisualElement();
                    item.name = $"Item{counter}";
                    item.AddToClassList("item"); // your USS class
                    items.Add(item);

                    // --- Details Button ---
                    var detailsBtn = new Button();
                    detailsBtn.name = $"Details{counter}";
                    detailsBtn.AddToClassList("item-details_button");
                    item.Add(detailsBtn);

                    // Header
                    var header = new VisualElement { name = "Header" };
                    header.AddToClassList("item-details-header");
                    detailsBtn.Add(header);

                    var headerLeft = new VisualElement();
                    headerLeft.style.flexShrink = 1;
                    headerLeft.style.flexGrow = 1;
                    header.Add(headerLeft);

                    var detailTitle = new Label($"{order.custname}") { name = "Detail_Title" };
                    detailTitle.AddToClassList("item-details-header-title");
                    detailTitle.style.fontSize = 16;
                    detailTitle.style.unityTextAlign = TextAnchor.UpperLeft;
                    detailTitle.style.whiteSpace = WhiteSpace.Normal;
                    headerLeft.Add(detailTitle);

                    var detailSubtitle = new Label($"{order.docuno}") { name = "Detail_Subtitle" };
                    detailSubtitle.AddToClassList("item-details-header-subtitle");
                    detailSubtitle.style.fontSize = 14;
                    detailSubtitle.style.unityTextAlign = TextAnchor.UpperLeft;
                    detailSubtitle.style.whiteSpace = WhiteSpace.Normal;
                    headerLeft.Add(detailSubtitle);

                    var detailRemark = new Label($"{order.remark}") { name = "Detail_Remark" };
                    detailRemark.AddToClassList("item-details-header-remark");
                    detailRemark.style.fontSize = 12;
                    detailRemark.style.unityTextAlign = TextAnchor.UpperLeft;
                    detailRemark.style.whiteSpace = WhiteSpace.Normal;
                    headerLeft.Add(detailRemark);

                    var listNo = new Label($"{counter}\n") { name = "ListNo" };
                    listNo.AddToClassList("item-details-header-list_no");
                    listNo.style.fontSize = 32;
                    listNo.style.unityFontStyleAndWeight = FontStyle.Bold;
                    listNo.style.unityTextAlign = TextAnchor.MiddleCenter;
                    listNo.style.whiteSpace = WhiteSpace.NoWrap;

                    header.Add(listNo);

                    //// Body
                    //var body = new VisualElement { name = "Body" };
                    //body.AddToClassList("item-details-body");
                    //detailsBtn.Add(body);

                    //for (int i = 0; i < 3; i++) // 3 items
                    //{
                    //    var itemRow = new VisualElement { name = "Item" };
                    //    body.Add(itemRow);

                    //    var listNum = new Label("1") { name = "List_Number" };
                    //    itemRow.Add(listNum);

                    //    var name = new Label("ยางนอก BS 225/75R15 R624") { name = "Name" };
                    //    itemRow.Add(name);

                    //    var amountUnit = new Label("4 เส้น") { name = "AmountNUnit" };
                    //    itemRow.Add(amountUnit);
                    //}

                    //// Footer
                    //var footer = new VisualElement { name = "Footer" };
                    //footer.AddToClassList("item-details-footer");
                    //detailsBtn.Add(footer);

                    //var shipment = new VisualElement { name = "Shipment" };
                    //footer.Add(shipment);

                    //var transportation = new VisualElement { name = "Transportation" };
                    //shipment.Add(transportation);

                    //var transportLabel = new Label("ขนส่งโดย") { name = "List_Number" };
                    //transportation.Add(transportLabel);

                    //var transportName = new Label("บ้านแพง (สาย3)") { name = "Name" };
                    //transportation.Add(transportName);

                    //var sticker = new VisualElement { name = "Sticker" };
                    //shipment.Add(sticker);

                    //var stickerDetail = new Label("เล็ก1 - 13 : 8 ชิ้น") { name = "Detail" };
                    //sticker.Add(stickerDetail);

                    //var stickerIcon = new VisualElement { name = "Icon" };
                    //sticker.Add(stickerIcon);

                    // Remove Button
                    var removeBtn = new Button();
                    removeBtn.name = $"Remove{counter}";
                    removeBtn.AddToClassList("item-remove_button");
                    item.Add(removeBtn);
                    removeBtn.clicked += () => RemoveOrder(order);

                    counter++;
                }
            } else {
                submitShipment.SetEnabled(false);
            }
        }
    }
}
