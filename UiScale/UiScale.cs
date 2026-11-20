using Timberborn.CoreUI;
using Timberborn.ModManagerScene;

namespace UiScale;

public class UiScaleModStarter : IModStarter
{
    public void StartMod(IModEnvironment environment) => new Harmony(nameof(CustomUiScale)).PatchAll();
}

public static class CustomUiScale
{
    [HarmonyPatch(typeof(UIScaler), nameof(UIScaler.ClampScaleFactor)), HarmonyPrefix]
    public static bool ClampScaleFactorPrefix(ref float __result, float value)
    {
        __result = 0.7f;

        return false;
    }

    [HarmonyPatch(typeof(UIScaler), nameof(UIScaler.SetScaleFactor)), HarmonyPrefix]
    public static bool SetScaleFactorPrefix(UIScaler __instance)
    {
        if (__instance is null || __instance._panelSettings is null || __instance._uiSettings is null)
        {
            UnityEngine.Debug.LogError("Cannot set scale");

            return true;
        }

        __instance._panelSettings.scale = 0.7f;

        return false;
    }
}