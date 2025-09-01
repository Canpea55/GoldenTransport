using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class SmoothScrolls : MonoBehaviour
{
    public string scrollClass = "smooth-scroll";
    public float scrollSpeed = 32;
    public float lerpSpeed = 8;
    public float dragSensitivity = 1f;
    private readonly Dictionary<ScrollView, float> targets = new();
    private readonly Dictionary<ScrollView, bool> isDragging = new();
    private readonly Dictionary<ScrollView, Vector2> lastMousePos = new();

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var scrollViews = root.Query<ScrollView>(className: scrollClass).ToList();

        foreach (var sv in scrollViews)
        {
            targets[sv] = 0f;
            isDragging[sv] = false;
            lastMousePos[sv] = Vector2.zero;

            sv.RegisterCallback<WheelEvent>(evt => OnWheel(evt, sv), TrickleDown.TrickleDown);
            sv.RegisterCallback<MouseDownEvent>(evt => OnMouseDown(evt, sv));
            sv.RegisterCallback<MouseMoveEvent>(evt => OnMouseMove(evt, sv));
            sv.RegisterCallback<MouseUpEvent>(evt => OnMouseUp(evt, sv));
            sv.RegisterCallback<MouseLeaveEvent>(evt => OnMouseLeave(evt, sv));
        }
    }

    private void OnWheel(WheelEvent evt, ScrollView sv)
    {
        if (!targets.ContainsKey(sv)) return;
        targets[sv] += evt.delta.y * scrollSpeed;
        evt.StopPropagation();
    }

    private void OnMouseDown(MouseDownEvent evt, ScrollView sv)
    {
        if (evt.button != 0) return; // Only left mouse button
        isDragging[sv] = true;
        lastMousePos[sv] = evt.mousePosition;
        sv.CaptureMouse();
        evt.StopPropagation();
    }

    private void OnMouseMove(MouseMoveEvent evt, ScrollView sv)
    {
        if (!isDragging[sv]) return;

        Vector2 delta = evt.mousePosition - lastMousePos[sv];
        targets[sv] -= delta.y * dragSensitivity; // Negative for natural drag feel
        lastMousePos[sv] = evt.mousePosition;
        evt.StopPropagation();
    }

    private void OnMouseUp(MouseUpEvent evt, ScrollView sv)
    {
        if (evt.button != 0) return;
        isDragging[sv] = false;
        sv.ReleaseMouse();
        evt.StopPropagation();
    }

    private void OnMouseLeave(MouseLeaveEvent evt, ScrollView sv)
    {
        isDragging[sv] = false;
        sv.ReleaseMouse();
    }

    void Update()
    {
        var keys = new List<ScrollView>(targets.Keys);

        foreach (var sv in keys)
        {
            var target = targets[sv];

            // Calculate max scroll using content container and viewport
            float contentHeight = sv.contentContainer.layout.height;
            float viewportHeight = sv.contentViewport.layout.height;
            float maxScroll = Mathf.Max(0, contentHeight - viewportHeight);

            float clampedTarget = Mathf.Clamp(target, 0, maxScroll);
            float current = sv.scrollOffset.y;
            float newVal = Mathf.Lerp(current, clampedTarget, Time.deltaTime * lerpSpeed);

            sv.scrollOffset = new Vector2(0, newVal);
            targets[sv] = clampedTarget;
        }
    }
}