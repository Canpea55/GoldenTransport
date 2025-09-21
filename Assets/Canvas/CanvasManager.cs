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
    public int sortOrder;
}

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [Header("Debug")]
    public bool startWithOverlay = false;
    public Screen previousScreen;
    [Header("Canvas")]
    public List<Screen> screens;
    public List<Screen> overlays;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);  // Optional: keep across scenes

        //setup
        foreach (Screen screen in screens)
        {
            if(screen.screenObject.activeSelf)
            {
                SetupManager(screen, false);
            }
            else
            {
                screen.screenObject.SetActive(true);
                SetupManager(screen, false);
            }
        }
        foreach (Screen o in overlays)
        {
            if(o.screenObject.activeSelf)
            {
                SetupManager(o, true);
            }
            else
            {
                o.screenObject.SetActive(true);
                SetupManager(o, true);
            }
        }
    }

    void SetupManager(Screen screen, bool isOverlay)
    {
        var uidoc = screen.screenObject.GetComponent<UIDocument>();
        if (uidoc != null)
        {
            var root = uidoc.rootVisualElement;
            var panel = root.Q<VisualElement>("Panel");
            panel.style.display = DisplayStyle.None;
            if (!isOverlay)
            {
                uidoc.sortingOrder = screen.sortOrder;
            }
            else
            {
                uidoc.sortingOrder = screen.sortOrder + 1024;
            }
            panel.SetEnabled(false);
        }
    }

    private void Start()
    {
        if (!startWithOverlay)
        {
            StartCoroutine(EnableScreen(screens[0].screenName));
            previousScreen = screens[0];
        } else
        {
            StartCoroutine(EnableScreen(overlays[0].screenName));
            previousScreen = overlays[0];
        }
    }

    public IEnumerator EnableScreen(string name, object data = null)
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

                    var controller = screen.screenObject.GetComponent<CanvasController>();
                    // deliver data BEFORE calling OnCanvasLoaded so the controller can use it during setup
                    if (controller != null && data != null)
                    {
                        try { controller.OnReceiveData(data); }
                        catch (Exception ex) { Debug.LogError($"OnReceiveData error for '{name}': {ex}"); }
                    }
                    if (controller != null)
                        controller.OnCanvasLoaded();

                    panel.style.display = DisplayStyle.Flex;
                    panel.SetEnabled(true);
                    yield break;
                }
                else
                {
                    Debug.LogError($"Screen : {name} not found!");
                    yield break;
                }
            }
        }
        Debug.LogError($"EnableScreen: screen '{name}' not registered in CanvasManager.screens");
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
                        screen.screenObject.GetComponent<CanvasController>().OnCanvasUnloaded();
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

    public IEnumerator EnableOverlay(string name, object data = null, Action<Dictionary<string, object>> onSubmit = null)
    {
        foreach (Screen overlay in overlays)
        {
            if (overlay.screenName == name)
            {
                var uidoc = overlay.screenObject.GetComponent<UIDocument>();
                var controller = overlay.screenObject.GetComponent<CanvasController>();

                if (overlay is IOverlayWithSubmit submitOverlay)
                {
                    submitOverlay.SetSubmitCallback(onSubmit);
                }

                if (uidoc != null)
                {
                    var root = uidoc.rootVisualElement;
                    var panel = root.Q<VisualElement>("Panel");

                    if (controller != null && data != null)
                    {
                        try { controller.OnReceiveData(data); }
                        catch (Exception ex) { Debug.LogError($"OnReceiveData error for overlay '{name}': {ex}"); }
                    }

                    panel.style.display = DisplayStyle.Flex;
                    panel.SetEnabled(true);
                    yield break;
                }
                else
                {
                    Debug.LogError($"Overlay : '{name}' not found!");
                    yield break;
                }
            }
        }
        Debug.LogError($"EnableOverlay: overlay '{name}' not registered in CanvasManager.overlays");
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
    public IEnumerator SwitchScreen(string fromName, string toName, long disableDelayMs, object data = null)
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
            yield return StartCoroutine(EnableScreen(toName, data));
            yield break;
        }

        // Start disabling the from screen (this schedules hiding after disableDelayMs)
        yield return StartCoroutine(DisableScreen(fromName, disableDelayMs));

        // Wait until the panel is actually hidden (resolvedStyle.display == None) or until timeout
        var fromUidoc = fromScreen.screenObject.GetComponent<UIDocument>();
        if (fromUidoc == null)
        {
            Debug.LogError($"SwitchScreen: UIDocument missing on from screen '{fromName}'. Enabling target.");
            yield return StartCoroutine(EnableScreen(toName, data));
            yield break;
        }

        var fromRoot = fromUidoc.rootVisualElement;
        var fromPanel = fromRoot.Q<VisualElement>("Panel");
        if (fromPanel == null)
        {
            Debug.LogError($"SwitchScreen: Panel element on from screen '{fromName}' not found. Enabling target.");
            yield return StartCoroutine(EnableScreen(toName, data));
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
        yield return StartCoroutine(EnableScreen(toName, data));
    }

    /// <summary>
    /// Convenience overload: finds the currently visible screen and switches to 'toName'.
    /// The currently visible screen is determined by checking Panel.resolvedStyle.display == Flex.
    /// Usage: StartCoroutine(SwitchScreen("main", 1000));
    /// </summary>
    public IEnumerator SwitchScreen(string toName, long disableDelayMs, object data = null)
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
            yield return StartCoroutine(EnableScreen(toName, data));
            yield break;
        }

        yield return StartCoroutine(SwitchScreen(current.screenName, toName, disableDelayMs, data));
    }

}
