using ModSettings.Common;
using ModSettings.CommonUI;
using ModSettings.Core;
using ModSettings.CoreUI;
using System;
using System.Globalization;
using Timberborn.CoreUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace UiScale;

internal class SliderFloatModSettingElementFactory : IModSettingElementFactory
{
    private readonly VisualElementLoader _visualElementLoader;
    private readonly ModSettingDescriptorInitializer _modSettingDescriptorInitializer;

    public SliderFloatModSettingElementFactory(VisualElementLoader visualElementLoader,
                                             ModSettingDescriptorInitializer
                                                 modSettingDescriptorInitializer)
    {
        _visualElementLoader = visualElementLoader;
        _modSettingDescriptorInitializer = modSettingDescriptorInitializer;
    }

    public int Priority => 100;

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is RangeFloatModSetting rangeFloatModSetting)
        {
            var root = _visualElementLoader.LoadVisualElement("ModSettings/SliderIntModSettingElement");
            _modSettingDescriptorInitializer.Initialize(root.Q<VisualElement>("Descriptor"),
                                                        rangeFloatModSetting);
            InitializeSlider(root, rangeFloatModSetting);
            element = new ModSettingElement(root, modSetting);
            return true;
        }
        element = null;
        return false;
    }

    private static void InitializeSlider(VisualElement root,
                                         RangeFloatModSetting rangeFloatModSetting)
    {
        var valueLabel = root.Q<Label>("Value");
        var slider = root.Q<Slider>("Slider");
        slider.lowValue = rangeFloatModSetting.MinValue;
        slider.highValue = rangeFloatModSetting.MaxValue;
        slider.value = rangeFloatModSetting.Value;
        slider.RegisterValueChangedCallback(v => {
            rangeFloatModSetting.SetValue((float)Math.Round(v.newValue, 2, MidpointRounding.AwayFromZero));
            valueLabel.text = rangeFloatModSetting.Value.ToString(CultureInfo.InvariantCulture);
        });
        valueLabel.text = rangeFloatModSetting.Value.ToString(CultureInfo.InvariantCulture);
        rangeFloatModSetting.ValueChanged += (_, newValue) => {
            slider.SetValueWithoutNotify(newValue);
            valueLabel.text = newValue.ToString(CultureInfo.InvariantCulture);
        };
    }
}
