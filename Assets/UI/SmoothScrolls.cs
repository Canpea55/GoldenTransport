using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class SmoothScrolls : MonoBehaviour
{
    public string scrollClass = "smooth-scroll"; // class selector in UXML/USS
    public float scrollSpeed = 32;
    public float lerpSpeed = 8;

    private readonly Dictionary<ScrollView, float> targets = new();

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Find all scrollviews with the given class
        var scrollViews = root.Query<ScrollView>(className: scrollClass).ToList();

        foreach (var sv in scrollViews)
        {
            targets[sv] = 0f; // init target offset
            sv.RegisterCallback<WheelEvent>(evt => OnWheel(evt, sv), TrickleDown.TrickleDown);
        }
    }

    private void OnWheel(WheelEvent evt, ScrollView sv)
    {
        if (!targets.ContainsKey(sv)) return;

        targets[sv] = Mathf.Clamp(
            targets[sv] + evt.delta.y * scrollSpeed,
            0,
            Mathf.Max(0, sv.contentContainer.layout.height - sv.layout.height)
        );

        evt.StopPropagation(); // block default jumpy scroll
    }

    void Update()
    {
        foreach (var kvp in targets)
        {
            var sv = kvp.Key;
            var target = kvp.Value;

            float current = sv.scrollOffset.y;
            float newVal = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);
            sv.scrollOffset = new Vector2(0, newVal);
        }
    }
}
