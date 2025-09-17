// TransportationsController.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[Serializable]
public class PivotData {
    public int shipment_id;
    public int order_id;
    public int list_number;
}

[Serializable]
public class OrderData {
    public int id;
    public string docuno;
    public string custname;
    public string remark;
    public string status;
    public PivotData pivot;
}

[Serializable]
public class ShipmentData {
    public int id;
    public string remark;
    public string vehicle_name;
    public string vehicle_color_hex;
    public string driver_name;
    public List<OrderData> orders;
}

[Serializable]
public class DateGroup {
    public string date;
    public List<ShipmentData> shipments;
}

public class TransportationsController : CanvasController
{
    public string apiUrl = "http://localhost:8000/api/shipments-json"; // <-- change to your URL
    private VisualElement ui;
    private ScrollView scroll;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        // wire existing controls if any
        var close = ui.Q<Button>("Close");
        if (close != null) close.clicked += OnCloseClicked;

        var addShipment = ui.Q<Button>("AddShipment");
        if (addShipment != null) addShipment.clicked += () => {
            StartCoroutine(CanvasManager.Instance.SwitchScreen("shipmentForm", 900));
        };

        // start loading
        StartCoroutine(LoadAndPopulate());
    }

    private IEnumerator LoadAndPopulate()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(apiUrl))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("Failed to fetch shipments: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            List<DateGroup> groups = JsonUtilityWrapper.FromJsonList<DateGroup>(json);
            BuildUI(groups);
        }
    }

    private void BuildUI(List<DateGroup> groups)
    {
        var tableData = ui.Q<VisualElement>("TableData");
        if (tableData == null)
        {
            Debug.LogError("TableData element not found in UXML.");
            return;
        }

        // find existing ScrollView inside TableData (from your UXML)
        scroll = tableData.Q<ScrollView>();
        if (scroll == null)
        {
            // create one if not present
            scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            tableData.Add(scroll);
        }

        // clear previous content
        scroll.contentContainer.Clear();

        foreach (var group in groups)
        {
            // VisualElement name="2025-09-13"
            var dateBlock = new VisualElement();
            dateBlock.name = $"date-{group.date}";
            dateBlock.AddToClassList("date_block");

            // VisualElement name="Date"
            var dateLabelOuter = new VisualElement();
            dateLabelOuter.AddToClassList("date_label_outer");

            var dateLabel = new Label(group.date);
            dateLabel.AddToClassList("date_label");
            dateLabelOuter.Add(dateLabel);

            dateBlock.Add(dateLabelOuter);

            // VisualElement name="Shipments"
            var shipmentsContainer = new VisualElement();
            shipmentsContainer.AddToClassList("shipments_container");
            dateBlock.Add(shipmentsContainer);

            foreach (var shipment in group.shipments)
            {
                // Row: left = driver and vehicle, right = orders VisualElement name="2025-09-13-2"
                var row = new VisualElement();
                row.name = $"{group.date}-{shipment.id}";
                row.AddToClassList("shipment");

                Color vehColor = new Color(206, 96, 89, 100);
                ColorUtility.TryParseHtmlString($"#{shipment.vehicle_color_hex}", out vehColor);
                Color vehColorSecondary = new Color(vehColor.r, vehColor.g, vehColor.b, 0.5f);

                // Left: Delivery (button) VisualElement name="Shipment"
                var left = new VisualElement();
                left.name = "Details";
                left.AddToClassList("shipment_vehicle");

                var deliveryBtn = new Button();
                deliveryBtn.name = $"{group.date}-{shipment.id}";
                deliveryBtn.AddToClassList("transportation-delivery-button");

                // driver-vehicle label e.g. "ปอง (รถสี่ 23-2456)"
                var driverVehicle = new Label($"{SafeText(shipment.driver_name)} ({SafeText(shipment.vehicle_name)})");
                driverVehicle.name = "title";
                driverVehicle.AddToClassList("shipment_vehicle_title");
                driverVehicle.style.color = vehColor;
                deliveryBtn.Add(driverVehicle);

                // remark label
                var remarkLabel = new Label(SafeText(shipment.remark));
                Debug.Log(remarkLabel.text);
                remarkLabel.name = "remark";
                remarkLabel.AddToClassList("shipment_vehicle_remark");
                deliveryBtn.Add(remarkLabel);

                // delivery click - navigate to shipment screen (you can modify to pass ID)
                int capturedShipmentId = shipment.id;
                deliveryBtn.clicked += () => {
                    Debug.Log($"Delivery clicked for shipment {capturedShipmentId}");
                    StartCoroutine(CanvasManager.Instance.SwitchScreen("shipment", 900));
                    // TODO: set selected shipment id somewhere if you need to show details
                };

                left.Add(deliveryBtn);

                // Right: Orders (column of buttons) VisualElement name="Orders"
                var right = new VisualElement();
                right.name = "Order";
                right.AddToClassList("shipment_orders");

                foreach (var order in shipment.orders)
                {
                    var orderBtn = new Button();
                    orderBtn.name = $"{group.date}-{shipment.id}-{order.id}";
                    orderBtn.AddToClassList("transportation-order-button");
                    orderBtn.AddToClassList("shipment_order-button");
                    orderBtn.style.backgroundColor = vehColor;

                    var custLabel = new Label(SafeText(order.custname));
                    custLabel.name = "Custname";
                    custLabel.AddToClassList("shipment_order-button-custname");
                    orderBtn.Add(custLabel);

                    var docLabel = new Label(SafeText(order.docuno));
                    docLabel.name = "Docuno";
                    docLabel.AddToClassList("shipment_order-button-docuno");
                    orderBtn.Add(docLabel);

                    var orderRemark = new Label(SafeText(order.remark));
                    orderRemark.name = "Remark";
                    orderRemark.AddToClassList("shipment_order-button-remark");
                    orderBtn.Add(orderRemark);

                    int capturedOrderId = order.id;
                    orderBtn.clicked += () => {
                        Debug.Log($"Order clicked: {capturedOrderId} (shipment {shipment.id})");
                        StartCoroutine(CanvasManager.Instance.EnableOverlay("orderDetails"));
                        // TODO: pass order id to overlay controller
                    };

                    right.Add(orderBtn);
                }

                row.Add(left);
                row.Add(right);

                shipmentsContainer.Add(row);
            }

            // add whole date block to scroll content
            scroll.contentContainer.Add(dateBlock);
        }
    }

    private void OnCloseClicked()
    {
        StartCoroutine(CanvasManager.Instance.SwitchScreen("main", 900));
    }

    private static string SafeText(string s)
    {
        return string.IsNullOrEmpty(s) ? "" : s;
    }
}

/// <summary>
/// Small helper to allow JsonUtility to parse a top-level JSON array.
/// Usage: JsonUtilityWrapper.FromJsonList<YourType>(jsonArrayString)
/// </summary>
public static class JsonUtilityWrapper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> list;
    }

    public static List<T> FromJsonList<T>(string json)
    {
        string newJson = "{\"list\":" + json + "}";
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.list ?? new List<T>();
    }
}
