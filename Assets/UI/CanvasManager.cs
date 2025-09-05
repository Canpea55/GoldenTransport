using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Screen
{
    public string screenName;
    public GameObject screenObject;
}

public class CanvasManager : MonoBehaviour
{
    public List<Screen> screens;

    private void Awake()
    {
        //setup
        foreach (Screen screen in screens)
        {
            var uidoc = screen.screenObject.GetComponent<UIDocument>();
            if (uidoc != null)
            {
                var root = uidoc.rootVisualElement;
                var panel = root.Q<VisualElement>("Panel");
                panel.style.display = DisplayStyle.None;
                panel.SetEnabled(false);
            }
        }
    }

    private void Start()
    {
        //enable main screen first
        StartCoroutine(EnableScreen("main"));
    }

    public IEnumerator EnableScreen(string name)
    {
        foreach (Screen screen in screens)
        {
            if (screen.screenName == name)
            {
                var uidoc = screen.screenObject.GetComponent<UIDocument>();
                if (uidoc != null)
                {
                    var root = uidoc.rootVisualElement;
                    var panel = root.Q<VisualElement>("Panel");

                    panel.style.display = DisplayStyle.Flex;
                    panel.SetEnabled(true);
                }
                else
                {
                    Debug.LogError($"Screen : {name} not found!");
                    yield break;
                }
            }
        }
    }

    public IEnumerator DisableScreen(string name, long duration)
    {
        foreach (Screen screen in screens)
        {
            if (screen.screenName == name)
            {
                var uidoc = screen.screenObject.GetComponent<UIDocument>();
                if (uidoc != null)
                {
                    var root = uidoc.rootVisualElement;
                    var panel = root.Q<VisualElement>("Panel");

                    panel.SetEnabled(false);
                    panel.schedule.Execute(() =>
                    {
                        panel.style.display = DisplayStyle.None;
                    }).StartingIn(duration);
                }
                else
                {
                    Debug.LogError($"Screen : {name} not found!");
                    yield break;
                }
            }
        }
    }

    //public IEnumerator SwitchScreen(string screenName, float duration)
    //{
    //    if (!uiScreens.ContainsKey(screenName))
    //    {
    //        Debug.LogError($"Screen {screenName} not found!");
    //        yield break;
    //    }
    //    else
    //    {
    //        // Switch screen
    //        //uiDocument.visualTreeAsset = uiScreens[screenName];
    //    }
    //}
}
