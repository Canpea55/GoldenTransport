// Attach to Camera GameObject
using UnityEngine;
using UnityEngine.UIElements;

public class SplitScreen : MonoBehaviour
{
    public CanvasManager canvasManager;
    public Camera mainCamera;  // 3D Camera
    public float cameraRectWidth = 0.3f;
    //public Camera uiCamera;    // UI Camera

    private void Start()
    {
        var screens = canvasManager.screens;
        foreach (var screen in screens)
        {
            if (screen.screenName == "transportation")
            {
                var screenObj = screen.screenObject;
                var root = screenObj.GetComponent<UIDocument>().rootVisualElement;
                VisualElement tdt = root.Q<VisualElement>("TransportDetail");
                if (tdt != null)
                {
                    Debug.Log(tdt.style.width);
                    return;
                }
                Debug.LogError("VisualElement:TransportDetail not found.");
            }
        }
        Debug.LogError("screen.screenName == \"transportation\" not found.");
    }

    void Update()
    {
        // 30% for 3D on left side
        mainCamera.rect = new Rect(0f, 0f, cameraRectWidth, 1f);

        // 70% for UI on right side
        //uiCamera.rect = new Rect(0.3f, 0f, 0.7f, 1f);
    }
}
