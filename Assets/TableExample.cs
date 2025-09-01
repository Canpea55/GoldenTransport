using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TableExample : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;

    class Sticker { public string folder_id; public int list_number; public string custname; public string description; }
    List<Sticker> data;

    void OnEnable()
    {
        var root = uiDoc.rootVisualElement;
        var table = root.Q<MultiColumnListView>("StickersTable");

        data = new List<Sticker> {
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },
            new Sticker{ folder_id="1", list_number=1, custname="KTDS", description="what the he" },

        };

        table.itemsSource = data;
        table.fixedItemHeight = 22; // optional, for tighter rows

        // Bind each column
        var colFolderid = table.columns["folder_id"];
        var colListnumber = table.columns["list_number"];
        var colCustname = table.columns["custname"];
        var colDescription = table.columns["description"];

        colFolderid.makeCell = () => new Label();
        colListnumber.makeCell = () => new Label();
        colCustname.makeCell = () => new Label();
        colDescription.makeCell = () => new Label();

        colFolderid.bindCell = (ve, i) => ((Label)ve).text = data[i].folder_id.ToString();
        colListnumber.bindCell = (ve, i) => ((Label)ve).text = data[i].list_number.ToString();
        colCustname.bindCell = (ve, i) => ((Label)ve).text = data[i].custname.ToString();
        colDescription.bindCell = (ve, i) => ((Label)ve).text = data[i].description.ToString();

        //// Optional: selection
        //table.selectionChanged += selected =>
        //{
        //    foreach (var item in selected) Debug.Log(((Sticker)item).custname);
        //};
    }
}
