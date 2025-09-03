using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class StickerTable : MonoBehaviour
{
    [SerializeField] private StickerAPI api; // drag StickerAPI component here in Inspector
    private UIDocument uiDoc;
    private string serverIp;
    List<Sticker> data = new List<Sticker>();
    List<Sticker> filteredData = new List<Sticker>();
    MultiColumnListView table;
    private TextField searchField;
    private bool isDataLoaded = false;

    void OnEnable()
    {
        uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        table = root.Q<MultiColumnListView>("StickersTable");
        searchField = root.Q<TextField>("SearchBox");

        // Initialize the table setup
        SetupTable();

        // Load data from server
        serverIp = PlayerPrefs.GetString("ServerIP");
        StartCoroutine(api.GetStickers(serverIp, OnStickersLoaded));
    }

    void SetupTable()
    {
        // Bind columns
        table.columns["folder_id"].makeCell = () => CreateRowCell();
        table.columns["list_number"].makeCell = () => CreateRowCell();
        table.columns["custname"].makeCell = () => CreateRowCell();
        table.columns["description"].makeCell = () => {
            var label = new Label();
            label.style.whiteSpace = WhiteSpace.Normal; // allow wrapping
            label.style.textOverflow = TextOverflow.Clip; // or Overflow if you want scrolling
            label.style.overflow = Overflow.Visible; // so it can grow
            label.style.flexWrap = Wrap.Wrap; // wraps within cell
            return label;
        };

        table.columns["folder_id"].bindCell = (ve, i) => BindRowCell(ve, filteredData[i], "folder_id");
        table.columns["list_number"].bindCell = (ve, i) => BindRowCell(ve, filteredData[i], "list_number");
        table.columns["custname"].bindCell = (ve, i) => BindRowCell(ve, filteredData[i], "custname");
        table.columns["description"].bindCell = (ve, i) => BindRowCell(ve, filteredData[i], "description");

        // Set up search functionality
        searchField.RegisterValueChangedCallback(evt =>
        {
            if (!isDataLoaded) return;

            string query = evt.newValue.ToLower();
            filteredData.Clear();

            if (string.IsNullOrEmpty(query))
            {
                filteredData.AddRange(data);
            }
            else
            {
                filteredData.AddRange(data.FindAll(st =>
                    st.cust_name.ToLower().Contains(query) ||
                    st.descriptions.ToLower().Contains(query)
                ));
            }

            table.itemsSource = filteredData;
            table.Rebuild();
        });

        // Set up selection handling
        table.selectionChanged += selected =>
        {
            foreach (var item in selected)
            {
                var sticker = (Sticker)item;
                Debug.Log("Row clicked: " + sticker.cust_name + " at " + sticker.list_number);
            }
        };
    }

    // Create a Label and wrap in a container with a row class
    Label CreateRowCell()
    {
        var label = new Label();
        label.AddToClassList("hover-anim"); // optional cell styling
        return label;
    }

    // Bind data to the cell
    void BindRowCell(VisualElement ve, Sticker item, string column)
    {
        var label = ve as Label;
        switch (column)
        {
            case "folder_id":
                label.text = item.folder_id.ToString();
                break;
            case "list_number":
                label.text = item.list_number.ToString();
                break;
            case "custname":
                label.text = item.cust_name;
                break;
            case "description":
                label.text = item.descriptions;
                break;
        }
    }

    void OnStickersLoaded(StickerList stickerList)
    {
        if (stickerList == null || stickerList.stickers == null)
        {
            Debug.LogWarning("No sticker data received from server");
            return;
        }

        data.Clear();
        filteredData.Clear();

        data.AddRange(stickerList.stickers);
        filteredData.AddRange(data);

        // Set the data source and rebuild
        table.itemsSource = filteredData;
        table.Rebuild();

        isDataLoaded = true;

        Debug.Log($"Loaded {data.Count} stickers from server");
    }
}