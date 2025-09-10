// Attach to Camera GameObject
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraRectManager : MonoBehaviour
{
    //public CanvasManager canvasManager;
    //public Camera mainCamera;  // 3D Camera
    //public Animator animator;
    //[Header("Functions")]
    //public bool shrink = false;
    //[Header("Camera Rect Settings")]
    //[Range(0f, 1f)] public float cameraRectX = 0f;
    //[Range(0f, 1f)] public float cameraRectY = 0f;
    //[Range(0f, 1f)] public float cameraRectWidth = 0.4f;
    //[Range(0f, 1f)] public float cameraRectHeight = 1f;

    private void Start()
    {
        //animator = GetComponent<Animator>();
        //animator.SetBool("Shrink", false);

        //var screens = canvasManager.screens;
        //foreach (var screen in screens)
        //{
        //    if (screen.screenName == "transportation")
        //    {
        //        var screenObj = screen.screenObject;
        //        var root = screenObj.GetComponent<UIDocument>().rootVisualElement;
        //        VisualElement tdt = root.Q<VisualElement>("TransportDetail");
        //        if (tdt != null)
        //        {
        //            Debug.Log(tdt.style.width);
        //            return;
        //        }
        //    }
        //}
    }

    void Update()
    {
        //mainCamera.rect = new Rect(cameraRectX, cameraRectY, cameraRectWidth, cameraRectHeight);

        //if (shrink)
        //{
        //    animator.SetBool("Shrink", true);
        //} else
        //{
        //    animator.SetBool("Shrink", false);
        //}
    }
}
