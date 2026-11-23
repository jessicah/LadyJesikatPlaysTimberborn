using Bindito.Core;
using ModSettings.Core;
using ModSettings.CoreUI;
using Timberborn.CoreUI;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
using UnityEngine;

namespace UiScale;

public class UiScaleSettings(ISettings settings, ModSettingsOwnerRegistry registry, ModRepository repository, UIScaler Scaler)
    : ModSettingsOwner(settings, registry, repository)
{
    protected override string ModId => "LadyJesikat.CustomUIScale";

    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public RangeFloatModSetting CustomUiScaleSetting => new(0.7f, 0.4f, 2.0f, ModSettingDescriptor.Create("LadyJesikat.CustomUIScale.CustomUiScale"), Scaler);
}

public class RangeFloatModSetting : ModSetting<float>
{
    public float MinValue { get; init; }
    public float MaxValue { get; init; }

    private UIScaler _scaler;

    public RangeFloatModSetting(float defaultValue, float minValue, float maxValue, ModSettingDescriptor descriptor, UIScaler scaler)
        : base(defaultValue, descriptor)
    {
        this.MinValue = minValue;
        this.MaxValue = maxValue;

        _scaler = scaler;
    }

    public override void SetValue(float value)
    {
        if (value == 0f)
        {
            Debug.LogError($"[UiScale] SetValue: Receive of 0f, ignoring");
            return;
        }

        Debug.LogWarning($"[UiScale] RangeFloatModSetting::SetValue({value})");

        base.SetValue(Mathf.Clamp(value, this.MinValue, this.MaxValue));

        Debug.LogWarning($"  Clamped to {base.Value}");

        _scaler._panelSettings.scale = base.Value;
    }
}

[Context("Game"), Context("MainMenu")]
public class UiScaleConfigurator : Configurator
{
    public override void Configure()
    {
        Bind<UiScaleSettings>().AsSingleton();
        MultiBind<IModSettingElementFactory>().To<SliderFloatModSettingElementFactory>().AsSingleton();
    }
}
