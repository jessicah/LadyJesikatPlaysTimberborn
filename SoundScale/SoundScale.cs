using System.Collections.Generic;
using System.Linq;
using Timberborn.ModManagerScene;
using Timberborn.SoundSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace SoundScale;

public class UiScaleModStarter : IModStarter
{
    public void StartMod(IModEnvironment environment) => new Harmony(nameof(SoundScalePitch)).PatchAll();
}

public class AudioSourceWithPitch
{
    public AudioSourceWithPitch(AudioSource audioSource, float originalPitch)
    {
        AudioSource = audioSource;
        OriginalPitch = originalPitch;
    }
    public AudioSource AudioSource { get; set; }
    public float OriginalPitch { get; set; }
}

public static class SoundScalePitch
{
    private static List<AudioSourceWithPitch> _audioSources = new List<AudioSourceWithPitch>();
    private static Dictionary<string, List<AudioSourceWithPitch>> _namedAudioSources = new();
    private static bool _isQuarterSpeed = false;

    [HarmonyPatch(typeof(Sounds), nameof(Sounds.CreateAudioSources)), HarmonyPostfix]
    public static void SoundsCreateAudioSourcesPostfix(Sounds __instance, string soundName, GameObject audioSourceRoot)
    {
        _audioSources.AddRange(__instance._sounds.Values.SelectMany(sound => sound).Select(audioSource => new AudioSourceWithPitch(audioSource, audioSource.pitch)));

        foreach (var source in __instance._sounds)
        {
            if (_namedAudioSources.ContainsKey(source.Key))
            {
                Debug.LogWarning($"Skipping {source.Key}, already exists in dictionary...");
            }
            else
            {
                _namedAudioSources.Add(source.Key,
                    source.Value.Select(audioSource => new AudioSourceWithPitch(audioSource, audioSource.pitch)).ToList());
            }
        }
    }

    [HarmonyPatch(typeof(Sounds), nameof(Sounds.InvalidateSounds)), HarmonyPostfix]
    public static void SoundsInvalidateSoundsPostfix(Sounds __instance)
    {
        _audioSources.Clear();
    }

#if false
    [HarmonyPatch(typeof(SoundEmitter), nameof(SoundEmitter.StartSound)), HarmonyPostfix]
    public static void SoundEmitterStartSoundPostfix(SoundEmitter __instance, ref AudioSource __result, string soundName, int priority, float spatialBlend, float delay)
    {
        if (CurrentSpeed == 0.25f)
        {
            Debug.LogWarning($"SoundEmitter::StartSound(): {soundName}: pitch = {__result.pitch}");

            if (__result is not null && __result.pitch >= 0.5f)
            {
                __result.pitch = __result.pitch / 4f;
            }
        }
    }

    [HarmonyPatch(typeof(LoopingSoundPlayer), nameof(LoopingSoundPlayer.PlayLooping)), HarmonyPostfix]
    public static void LoopingSoundPlayerPlayLoopingPostfix(LoopingSoundPlayer __instance, string soundName, int priority, float spatialBlend, Vector3 offset)
    {

        if (__instance._currentlyPlaying.pitch >= 0.5f)
        {
            __instance._currentlyPlaying.pitch = __instance._currentlyPlaying.pitch / 4f;
        }
    }
#endif

    public static float CurrentSpeed { get; set; } = 1f;

    [HarmonyPatch(typeof(SpeedManager), nameof(SpeedManager.ChangeSpeed), new System.Type[] { }), HarmonyPostfix]
    public static void SpeedManagerChangeSpeedPostfix(SpeedManager __instance)
    {
        CurrentSpeed = __instance.CurrentSpeed;

        if (CurrentSpeed == 0.25f)
        {
            Debug.LogWarning($"SpeedManager::ChangeSpeed(): CurrentSpeed = {CurrentSpeed}: Adjusting audio source pitches");
            foreach (var audioSourceWithPitch in _audioSources)
            {
                if (audioSourceWithPitch.AudioSource is not null)
                {
                    audioSourceWithPitch.AudioSource.pitch = audioSourceWithPitch.OriginalPitch / 4f;
                }
            }
        }
        else
        {
            Debug.LogWarning($"SpeedManager::ChangeSpeed(): CurrentSpeed = {CurrentSpeed}: Restoring audio source pitches");
            foreach (var audioSourceWithPitch in _audioSources)
            {
                if (audioSourceWithPitch.AudioSource is not null)
                {
                    audioSourceWithPitch.AudioSource.pitch = audioSourceWithPitch.OriginalPitch;
                }
            }
        }
    }
}
