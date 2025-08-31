using UnityEngine;
using UnityEngine.UIElements;

public class PercentageField : VisualElement, INotifyValueChanged<int>
{
    public new class UxmlFactory : UxmlFactory<PercentageField, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription m_Value = new() { name = "value", defaultValue = 0 };
        UxmlStringAttributeDescription m_Label = new() { name = "label", defaultValue = "" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var self = (PercentageField)ve;
            self.SetValueWithoutNotify(Mathf.Clamp(m_Value.GetValueFromBag(bag, cc), 0, 100));
        }
    }

    // USS class names
    public static readonly string ussRoot = "pct-field";
    public static readonly string ussLabel = "pct-field__label";
    public static readonly string ussRow = "pct-field__row";
    public static readonly string ussText = "pct-field__text";
    public static readonly string ussSlider = "pct-field__slider";

    private IntegerField _text;
    private SliderInt _slider;

    private int _value;

    public PercentageField()
    {
        AddToClassList(ussRoot);

        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.AddToClassList(ussRow);
        Add(row);

        _text = new IntegerField() { name = "text" };
        _text.isDelayed = true;  // commit on enter/tab
        _text.AddToClassList(ussText);
        row.Add(_text);

        _slider = new SliderInt(0, 100) { name = "slider" };
        _slider.style.flexGrow = 1;
        _slider.AddToClassList(ussSlider);
        row.Add(_slider);

        // Wire child changes -> this control
        _text.RegisterValueChangedCallback(evt =>
        {
            value = Mathf.Clamp(evt.newValue, 0, 100);
        });
        _slider.RegisterValueChangedCallback(evt =>
        {
            value = evt.newValue;
        });

        // Initialize internal state without sending events
        SetValueWithoutNotify(0);
    }

    // INotifyValueChanged<int>
    public int value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            using var evt = ChangeEvent<int>.GetPooled(_value, value);
            evt.target = this;
            SetValueWithoutNotify(value);
            SendEvent(evt);
        }
    }

    public void SetValueWithoutNotify(int newValue)
    {
        _value = Mathf.Clamp(newValue, 0, 100);
        // update children without loops
        if (_text.value != _value) _text.SetValueWithoutNotify(_value);
        if (_slider.value != _value) _slider.SetValueWithoutNotify(_value);
    }
}
