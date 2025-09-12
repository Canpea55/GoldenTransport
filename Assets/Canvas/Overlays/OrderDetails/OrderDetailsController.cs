using UnityEngine;
using UnityEngine.UIElements;

public class OrderDetailsController : CanvasController
{
    public VisualElement ui;

    public Button close;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        close = ui.Q<Button>("Close");
        close.clicked += () =>
        {
            StartCoroutine(CanvasManager.Instance.DisableOverlay("orderDetails", 600));
        };
    }
}
