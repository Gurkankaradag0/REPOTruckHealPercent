using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using TruckHealPercent.Config;
using TruckHealPercent.Patches;
using UnityEngine;

namespace TruckHealPercent
{
    [BepInPlugin("Ezentere.TruckHealPercent", "Truck Heal Percent", "2.0.0")]
    public class TruckHealPercent : BaseUnityPlugin
    {
        internal static TruckHealPercent Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }

        public static ConfigManager configManager = null!;
        public static ConfigEntry<int> ConfigHealPercent = null!;
        public static int? HealPercent = null!;

        private void Awake()
        {
            Instance = this;

            // Prevent the plugin from being deleted
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            Patch();

            ConfigHealPercent = Config.Bind("General", "PercentAmount", 25, new ConfigDescription("The percentage of health the truck healer restores to the player.", (AcceptableValueBase)(object)new AcceptableValueRange<int>(0, 100), Array.Empty<object>()));
            HealPercent = ConfigHealPercent.Value;
            Logger.LogInfo($"{Info.Metadata.Name} v{Info.Metadata.Version} is loading with percent amount: {HealPercent}!");
        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
            Harmony.PatchAll(typeof(PlayerAvatarPatch));
            Harmony.PatchAll(typeof(NetworkConnectPatch));
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }

        private void Update()
        {
            // Code that runs every frame goes here
        }
    }
}