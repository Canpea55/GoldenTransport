// TransportationsController.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;
using Button = UnityEngine.UIElements.Button;

[Serializable]
public class DateGroup
{
    public string date;
    public List<Shipment> shipments;
}

public class TransportationsController : CanvasController
{
    private string apiUrl;
    private VisualElement ui;
    private ScrollView scroll;
    private List<DateGroup> allGroups; // To store all the data from the server

    public override void OnCanvasLoaded()
    {
        apiUrl = "http://" + PlayerPrefs.GetString("ServerIP") + "/api/shipments"; // <-- change to your URL
        base.OnCanvasLoaded();

        // wire existing controls if any
        var close = ui.Q<Button>("Close");
        if (close != null) close.clicked += OnCloseClicked;

        var addShipment = ui.Q<Button>("AddShipment");
        var data = new Dictionary<string, object>
        {
            { "type", "add" }
        };
        if (addShipment != null) addShipment.clicked += () => {
            StartCoroutine(CanvasManager.Instance.SwitchScreen("shipmentForm", 900, data));
        };

        // Search Box logic
        var searchTextField = ui.Q<TextField>("SearchTextField");
        if (searchTextField != null)
        {
            searchTextField.RegisterValueChangedCallback(evt => FilterAndBuildUI(evt.newValue));
        }


        // start loading
        StartCoroutine(LoadAndPopulate(false));
    }

    public override void OnCanvasUnloaded()
    {
        base.OnCanvasUnloaded();
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
    }

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private IEnumerator LoadAndPopulate(bool silent)
    {
        VisualElement data = ui.Q<VisualElement>("TableData");
        VisualElement nodata = ui.Q<VisualElement>("NoDataMessage");

        if(!silent) StartCoroutine(CanvasManager.Instance.EnableOverlay("loading"));
        using (UnityWebRequest req = UnityWebRequest.Get(apiUrl))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                //try again
                Debug.LogWarning("Failed to fetch shipments trying again.");
                using (UnityWebRequest req2 = UnityWebRequest.Get(apiUrl))
                {
                    req2.timeout = 10;
                    yield return req2.SendWebRequest();
                }

                data.style.display = DisplayStyle.None;
                nodata.style.display = DisplayStyle.Flex;
                StartCoroutine(CanvasManager.Instance.DisableOverlay("loading", 600));
                Debug.LogError($"Failed to fetch shipments: {apiUrl} " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            allGroups = JsonUtilityWrapper.FromJsonList<DateGroup>(json); // Store in allGroups
            BuildUI(allGroups);
        }

        data.style.display = DisplayStyle.Flex;
        nodata.style.display = DisplayStyle.None;
        if(!silent) StartCoroutine(CanvasManager.Instance.DisableOverlay("loading", 600));
    }

    private void FilterAndBuildUI(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            BuildUI(allGroups);
            return;
        }

        var filteredGroups = new List<DateGroup>();
        var lowerSearchText = searchText.ToLower();

        foreach (var group in allGroups)
        {
            if (group.date.ToLower().Contains(lowerSearchText))
            {
                filteredGroups.Add(group);
            }
            else
            {
                var matchingShipments = new List<Shipment>();
                foreach (var shipment in group.shipments)
                {
                    bool shipmentMatch = false;
                    if (shipment.driver_name.ToLower().Contains(lowerSearchText) ||
                        shipment.vehicle_name.ToLower().Contains(lowerSearchText))
                    {
                        shipmentMatch = true;
                    }

                    var matchingOrders = shipment.orders.Where(order =>
                        order.custname.ToLower().Contains(lowerSearchText) ||
                        order.docuno.ToLower().Contains(lowerSearchText) ||
                        order.remark.ToLower().Contains(lowerSearchText)
                    ).ToList();

                    if (shipmentMatch || matchingOrders.Any())
                    {
                        var newShipment = new Shipment
                        {
                            id = shipment.id,
                            driver_name = shipment.driver_name,
                            vehicle_name = shipment.vehicle_name,
                            vehicle_color_hex = shipment.vehicle_color_hex,
                            remark = shipment.remark,
                            orders = shipmentMatch ? shipment.orders : matchingOrders // if shipment matches, show all orders, otherwise show only matching orders.
                        };
                        matchingShipments.Add(newShipment);
                    }
                }

                if (matchingShipments.Any())
                {
                    filteredGroups.Add(new DateGroup { date = group.date, shipments = matchingShipments });
                }
            }
        }
        BuildUI(filteredGroups);
    }

    private void BuildUI(List<DateGroup> groups)
    {
        var tableData = ui.Q<VisualElement>("TableData");
        var noDataMessage = ui.Q<VisualElement>("NoDataMessage");

        if (tableData == null)
        {
            Debug.LogError("TableData element not found in UXML.");
            return;
        }
        if (noDataMessage == null)
        {
            Debug.LogError("NoDataMessage element not found in UXML.");
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

        if (groups == null || groups.Count == 0)
        {
            // No data, show the message and hide the table
            tableData.style.display = DisplayStyle.None;
            noDataMessage.style.display = DisplayStyle.Flex;
        }
        else
        {
            tableData.style.display = DisplayStyle.Flex;
            noDataMessage.style.display = DisplayStyle.None;

            foreach (var group in groups)
            {
                // VisualElement name="2025-09-13"
                var dateBlock = new VisualElement();
                dateBlock.name = $"date-{group.date}";
                dateBlock.AddToClassList("date_block");

                // VisualElement name="Date"
                var dateLabelOuter = new VisualElement();
                dateLabelOuter.AddToClassList("date_label_outer");

                DateTime localDate;
                DateTime.TryParseExact(group.date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out localDate);
                var dateLabel = new Label(localDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
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

                        var detailGrp = new VisualElement();
                        detailGrp.AddToClassList("shipment_order-button-detailgrp");

                        var custLabel = new Label(SafeText(order.custname));
                        custLabel.name = "Custname";
                        custLabel.AddToClassList("shipment_order-button-custname");
                        detailGrp.Add(custLabel);

                        var docLabel = new Label(SafeText(order.docuno));
                        docLabel.name = "Docuno";
                        docLabel.AddToClassList("shipment_order-button-docuno");
                        detailGrp.Add(docLabel);

                        var orderRemark = new Label(SafeText(order.remark));
                        orderRemark.name = "Remark";
                        orderRemark.AddToClassList("shipment_order-button-remark");
                        detailGrp.Add(orderRemark);

                        orderBtn.Add(detailGrp);

                        var checkbox = new Toggle();
                        if (order.status == "completed")
                        {
                            checkbox.value = true;
                            var a = new Color();
                            ColorUtility.TryParseHtmlString($"#27AE60", out a);
                            orderBtn.style.backgroundColor = a;
                        }
                        checkbox.name = $"TG{group.date}-{shipment.id}-{order.id}";
                        checkbox.AddToClassList("transportation-order-checkbox");
                        checkbox.RegisterValueChangedCallback(evt =>
                        {
                            StartCoroutine(ToggleStatus(order));
                            var b = vehColor;
                            if (order.status == "completed")
                            {
                                var a = new Color();
                                ColorUtility.TryParseHtmlString($"#27AE60", out a);
                                orderBtn.style.backgroundColor = a;
                            }
                            else
                            {
                                orderBtn.style.backgroundColor = b;
                            }
                        });
                        orderBtn.Add(checkbox);


                        int capturedOrderId = order.id;
                        var data = new Dictionary<string, object>
                        {
                            { "overlayType", "edit" },
                            { "order", order },
                        };
                        orderBtn.clicked += () =>
                        {
                            StartCoroutine(CanvasManager.Instance.EnableOverlay("orderDetails", data, (result) =>
                            {
                                var orderData = result["orderData"];
                                var type = orderData.GetType();

                                // Check if property "delete" exists
                                var deleteProp = type.GetProperty("delete");

                                if (deleteProp != null && (bool)deleteProp.GetValue(orderData))
                                {
                                    // --- Delete Order ---
                                    int id = (int)type.GetProperty("id").GetValue(orderData);
                                    Order o = new Order { id = id };

                                    StartCoroutine(DeleteOrder(o));
                                }
                                else
                                {
                                    // --- Update Order ---
                                    int id = (int)type.GetProperty("id").GetValue(orderData);
                                    string docuno = (string)type.GetProperty("docuno").GetValue(orderData);
                                    string custname = (string)type.GetProperty("custname").GetValue(orderData);
                                    string remark = (string)type.GetProperty("remark").GetValue(orderData);
                                    string status = (string)type.GetProperty("status").GetValue(orderData);

                                    Order o = new Order
                                    {
                                        id = id,
                                        docuno = docuno,
                                        custname = custname,
                                        remark = remark,
                                        status = status
                                    };

                                    custLabel.text = custname;
                                    docLabel.text = docuno;
                                    orderRemark.text = remark;

                                    StartCoroutine(UpdateOrder(o));
                                    StartCoroutine(LoadAndPopulate(true));
                                }
                            }));
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

    }

    IEnumerator ToggleStatus(Order order)
    {

        switch (order.status)
        {
            case "pending":
                order.status = "completed";
                break;
            case "completed":
                order.status = "pending";
                break;
        }

        using (UnityWebRequest req = new UnityWebRequest("http://" + SettingsManager.Instance.GetServerIP() + "/api/order/status", "POST"))
        {
            string json = JsonUtility.ToJson(order);
            Debug.Log(json);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error toggle status order: " + req.error + " - " + req.downloadHandler.text);
            }
            else
            {
                Debug.Log("Toggle order's status successfully: " + req.downloadHandler.text);
            }
        }
    }

    IEnumerator DeleteOrder(Order order)
    {
        using (UnityWebRequest req = new UnityWebRequest("http://" + SettingsManager.Instance.GetServerIP() + "/api/order", "DELETE"))
        {
            string json = JsonUtility.ToJson(order);
            Debug.Log(json);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error deleting order: " + req.error + " - " + req.downloadHandler.text);
            }
            else
            {
                Debug.Log("Order deleted successfully: " + req.downloadHandler.text);
            }
        }
    }

    IEnumerator UpdateOrder(Order order)
    {
        using (UnityWebRequest req = new UnityWebRequest("http://" + SettingsManager.Instance.GetServerIP() + "/api/order", "POST"))
        {
            string json = JsonUtility.ToJson(order);
            Debug.Log(json);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error posting order: " + req.error + " - " + req.downloadHandler.text);
            }
            else
            {
                Debug.Log("Order updated successfully: " + req.downloadHandler.text);
            }
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