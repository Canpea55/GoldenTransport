using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StickerTable : MonoBehaviour
{
    [SerializeField] private StickerAPI api; // drag StickerAPI component here in Inspector
    private UIDocument uiDoc;
    private string serverIp;

    List<Sticker> data;
    MultiColumnListView table;

    void OnEnable()
    {
        uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        table = root.Q<MultiColumnListView>("StickersTable");

        data = new List<Sticker>();
        table.itemsSource = data;

        // Bind columns
        table.columns["folder_id"].makeCell = () => CreateRowCell();
        table.columns["list_number"].makeCell = () => CreateRowCell();
        table.columns["custname"].makeCell = () => CreateRowCell();
        table.columns["description"].makeCell = () => CreateRowCell();

        table.columns["folder_id"].bindCell = (ve, i) => BindRowCell(ve, data[i], "folder_id");
        table.columns["list_number"].bindCell = (ve, i) => BindRowCell(ve, data[i], "list_number");
        table.columns["custname"].bindCell = (ve, i) => BindRowCell(ve, data[i], "custname");
        table.columns["description"].bindCell = (ve, i) => BindRowCell(ve, data[i], "description");

        serverIp = PlayerPrefs.GetString("ServerIP");
        StartCoroutine(api.GetStickers(serverIp, OnStickersLoaded));
    }

    // Create a Label and wrap in a container with a row class
    Label CreateRowCell()
    {
        var label = new Label();
        label.AddToClassList("hover-anim"); // optional cell styling
        return label;
    }

    // Bind data to the cell and add row class
    void BindRowCell(VisualElement ve, Sticker item, string column)
    {
        var label = ve as Label;
        switch (column)
        {
            case "folder_id": label.text = item.folder_id.ToString(); break;
            case "list_number": label.text = item.list_number.ToString(); break;
            case "custname": label.text = item.cust_name; break;
            case "description": label.text = item.descriptions; break;
        }

        // add row class to the parent (row container)
        //ve.parent?.AddToClassList("hover-anim");
    }

    void OnStickersLoaded(StickerList stickerList)
    {
        if (stickerList == null) return;

        data.Clear();
        data.AddRange(stickerList.stickers);

        //When a row is clicked or selected
        table.selectionChanged += selected =>
        {
            foreach (var item in selected)
            {
                var sticker = (Sticker)item;
                Debug.Log("Row clicked: " + sticker.cust_name + " at " + sticker.list_number);
            }
        };

        // refresh UI
        table.Rebuild();
    }
}
