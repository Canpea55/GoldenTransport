using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingController : CanvasController
{
    public VisualElement ui;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {

    }

}
