using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using HarmonyLib.Tools;

namespace JesterTimeout;

[BepInPlugin("Rosentti.JesterTimeout", "JesterTimeout", "1.0.1")]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<int> TimeoutSecondsConfig { get; private set; }

    private void Awake()
    {
        TimeoutSecondsConfig = base.Config.Bind(
            "General",                          // Config section
            "TimeoutSeconds",                   // Key of this config
            60,                               // Default value
            "How many seconds the Jester will be active until it's put back in it's box"    // Description
        );

        Harmony.CreateAndPatchAll(typeof(PatchClass));
    }
}