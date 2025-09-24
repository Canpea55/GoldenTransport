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

        close.clicked += Close;
        addOrder.clicked += AddOrder;
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
        StartCoroutine(canvasManager.SwitchScreen(canvasManager.previousScreen.screenName, 300));
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

    public void UpdateUI()
    {
        var items = ui.Q<VisualElement>("Items");
        if (items != null)
        {
            items.Clear();

            if (orders.Count > 0)
            {
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

                    //    var name = new Label("�ҧ�͡ BS 225/75R15 R624") { name = "Name" };
                    //    itemRow.Add(name);

                    //    var amountUnit = new Label("4 ���") { name = "AmountNUnit" };
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

                    //var transportLabel = new Label("������") { name = "List_Number" };
                    //transportation.Add(transportLabel);

                    //var transportName = new Label("��ҹᾧ (���3)") { name = "Name" };
                    //transportation.Add(transportName);

                    //var sticker = new VisualElement { name = "Sticker" };
                    //shipment.Add(sticker);

                    //var stickerDetail = new Label("���1 - 13 : 8 ���") { name = "Detail" };
                    //sticker.Add(stickerDetail);

                    //var stickerIcon = new VisualElement { name = "Icon" };
                    //sticker.Add(stickerIcon);

                    // Remove Button
                    var removeBtn = new Button();
                    removeBtn.name = $"Remove{counter}";
                    removeBtn.AddToClassList("item-remove_button");
                    item.Add(removeBtn);

                    counter++;
                }
            }
        }
    }
}
