using UnityEngine;
using UnityEngine.UIElements;

public class ShipmentController : CanvasController
{
    CanvasManager canvasManager;
    public VisualElement ui;

    public Camera cam;

    public Button close;

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
        close.clicked += () =>
        {
            StartCoroutine(canvasManager.SwitchScreen(canvasManager.previousScreen.screenName, 300));
        };
    }

    public override void OnCanvasLoaded()
    {
        //cam.enabled = true;
        cam.enabled = false;
    }

    public override void OnCanvasUnloaded()
    {
        cam.enabled = false;
    }   
}
