using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UIElements;

[Serializable]
public class Screen
{
    public string screenName;
    public GameObject screenObject;
}

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    public List<Screen> screens;
    public List<Screen> overlays;
    public Screen previousScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // Optional: keep across scenes

        //setup
        foreach (Screen screen in screens)
        {
            if(screen.screenObject.activeSelf)
            {
                SetupManager(screen);
            }
            else
            {
                screen.screenObject.SetActive(true);
                SetupManager(screen);
            }
        }
    }

    void SetupManager(Screen screen)
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

    private void Start()
    {
        //enable main screen first
        StartCoroutine(EnableScreen("main"));
        previousScreen = screens[0];
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
                    screen.screenObject.GetComponent<CanvasController>().OnCanvasLoaded();
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
                        screen.screenObject.GetComponent<CanvasController>().OnCanvasUnloaded();
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

    public IEnumerator EnableOverlay(string name)
    {
        foreach (Screen overlay in overlays)
        {
            if (overlay.screenName == name)
            {
                var uidoc = overlay.screenObject.GetComponent<UIDocument>();
                if (uidoc != null)
                {
                    var root = uidoc.rootVisualElement;
                    var panel = root.Q<VisualElement>("Panel");

                    panel.style.display = DisplayStyle.Flex;
                    panel.SetEnabled(true);
                }
                else
                {
                    Debug.LogError($"Overlay : '{name}' not found!");
                    yield break;
                }
            }
        }
    }

    public IEnumerator DisableOverlay(string name, long duration)
    {
        foreach (Screen overlay in overlays)
        {
            if (overlay.screenName == name)
            {
                var uidoc = overlay.screenObject.GetComponent<UIDocument>();
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
                    Debug.LogError($"Screen : '{name}' not found!");
                    yield break;
                }
            }
        }
    }
    // ---------------------------
    // SWITCH SCREEN
    // ---------------------------

    /// <summary>
    /// Switch from one screen to another WITHOUT crossfade.
    /// It calls DisableScreen(fromName, disableDelayMs) (which schedules hiding),
    /// waits until the fromPanel is actually hidden (or a safety timeout),
    /// then calls EnableScreen(toName).
    /// disableDelayMs is in milliseconds (matches StartingIn()).
    /// Usage: StartCoroutine(SwitchScreen("transportation", "main", 1000));
    /// </summary>
    public IEnumerator SwitchScreen(string fromName, string toName, long disableDelayMs)
    {
        // find screens
        Screen fromScreen = screens.Find(s => s.screenName == fromName);
        Screen toScreen = screens.Find(s => s.screenName == toName);
        previousScreen = fromScreen;

        if (toScreen == null)
        {
            Debug.LogError($"SwitchScreen: to screen '{toName}' not found!");
            yield break;
        }

        // If from screen not found, just enable target immediately
        if (fromScreen == null)
        {
            Debug.LogWarning($"SwitchScreen: from screen '{fromName}' not found. Enabling target directly.");
            yield return StartCoroutine(EnableScreen(toName));
            yield break;
        }

        // Start disabling the from screen (this schedules hiding after disableDelayMs)
        yield return StartCoroutine(DisableScreen(fromName, disableDelayMs));

        // Wait until the panel is actually hidden (resolvedStyle.display == None) or until timeout
        var fromUidoc = fromScreen.screenObject.GetComponent<UIDocument>();
        if (fromUidoc == null)
        {
            Debug.LogError($"SwitchScreen: UIDocument missing on from screen '{fromName}'. Enabling target.");
            yield return StartCoroutine(EnableScreen(toName));
            yield break;
        }

        var fromRoot = fromUidoc.rootVisualElement;
        var fromPanel = fromRoot.Q<VisualElement>("Panel");
        if (fromPanel == null)
        {
            Debug.LogError($"SwitchScreen: Panel element on from screen '{fromName}' not found. Enabling target.");
            yield return StartCoroutine(EnableScreen(toName));
            yield break;
        }

        // Convert ms to seconds for timeout
        float timeoutSeconds = (disableDelayMs / 1000f) + 0.2f; // small safety margin
        float elapsed = 0f;

        // Poll until resolvedStyle.display is None (hidden) or timeout
        while (fromPanel.resolvedStyle.display != DisplayStyle.None && elapsed < timeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // If still visible after timeout, log a warning but proceed
        if (fromPanel.resolvedStyle.display != DisplayStyle.None)
        {
            Debug.LogWarning($"SwitchScreen: fromPanel '{fromName}' not hidden after {timeoutSeconds} seconds; proceeding to enable '{toName}'.");
        }

        // Finally enable the target screen
        yield return StartCoroutine(EnableScreen(toName));
    }

    /// <summary>
    /// Convenience overload: finds the currently visible screen and switches to 'toName'.
    /// The currently visible screen is determined by checking Panel.resolvedStyle.display == Flex.
    /// Usage: StartCoroutine(SwitchScreen("main", 1000));
    /// </summary>
    public IEnumerator SwitchScreen(string toName, long disableDelayMs)
    {
        // find the currently visible screen (first one with Panel.resolvedStyle.display == Flex)
        Screen current = null;
        foreach (var s in screens)
        {
            var uidoc = s.screenObject.GetComponent<UIDocument>();
            if (uidoc == null) continue;
            var root = uidoc.rootVisualElement;
            var panel = root.Q<VisualElement>("Panel");
            if (panel == null) continue;
            if (panel.resolvedStyle.display == DisplayStyle.Flex)
            {
                current = s;
                break;
            }
        }

        if (current == null)
        {
            // No visible screen found -> enable directly
            yield return StartCoroutine(EnableScreen(toName));
            yield break;
        }

        yield return StartCoroutine(SwitchScreen(current.screenName, toName, disableDelayMs));
    }

}
