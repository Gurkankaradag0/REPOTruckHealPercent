using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using TruckHealPercent.Patches;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace TruckHealPercent
{
    [BepInPlugin("Ezentere.TruckHealPercent", "Truck Heal Percent", "1.0.0")]
    public class TruckHealPercent : BaseUnityPlugin
    {
        internal static TruckHealPercent Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }
        private static ConfigManager config_manager = null!;
        public static ConfigEntry<int> ConfigHealPercent = null!;
        public static int? HealPercent = null;

        private void Awake()
        {
            Instance = this;

            // Prevent the plugin from being deleted
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            Patch();

            ConfigHealPercent = Config.Bind("General", "PercentAmount", 25, new ConfigDescription("The percentage of health the truck healer restores to the player.", (AcceptableValueBase)(object)new AcceptableValueRange<int>(0, 100), Array.Empty<object>()));
            HealPercent = ConfigHealPercent.Value;
            Logger.LogInfo($"{Info.Metadata.Name} v{Info.Metadata.Version} is loading with percent amount: {ConfigHealPercent.Value}!");

            config_manager = new ConfigManager();
            ConfigHealPercent.SettingChanged += config_manager.OnConfigUpdate;
        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
            Harmony.PatchAll(typeof(PlayerAvatarPatch));
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }

        private void Update()
        {
            // Code that runs every frame goes here
        }

        [HarmonyPatch(typeof(NetworkConnect), "TryJoiningRoom")]
        public class JoinLobbyPatch
        {
            private static void Prefix()
            {
                PhotonNetwork.AddCallbackTarget(config_manager);
            }
        }

    }

    public class ConfigManager : MonoBehaviourPunCallbacks
    {
        public override void OnCreatedRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var props = PhotonNetwork.CurrentRoom.CustomProperties;
                props["HealPercent"] = (int)TruckHealPercent.ConfigHealPercent.Value;
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                TruckHealPercent.Logger.LogDebug("I'm the host. Add room mod properties.");
            }
        }
        public override void OnJoinedRoom()
        {
            object value;
            TruckHealPercent.Logger.LogDebug("Room joined");
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("HealPercent", out value))
            {
                TruckHealPercent.HealPercent = (int)value;
                TruckHealPercent.Logger.LogInfo($"Host heal value: {TruckHealPercent.HealPercent}%");
            }
            else
            {
                TruckHealPercent.HealPercent = null;
                TruckHealPercent.Logger.LogWarning("The host did not send settings for the mod. The healing will work in vanilla mode.");
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            object value;
            TruckHealPercent.Logger.LogDebug("Room propirties updated");
            if (propertiesThatChanged.TryGetValue("HealPercent", out value))
            {
                TruckHealPercent.HealPercent = (int)value;
                TruckHealPercent.Logger.LogInfo($"Host heal value: {TruckHealPercent.HealPercent}%");
            }
            else
            {
                TruckHealPercent.Logger.LogDebug("The host did not send settings for the mod. Just ignore.");
            }
        }

        public override void OnLeftRoom()
        {
            TruckHealPercent.HealPercent = TruckHealPercent.ConfigHealPercent.Value;
        }

        public void OnConfigUpdate(object sender, EventArgs e)
        {
            TruckHealPercent.Logger.LogDebug($"Config updated: {TruckHealPercent.ConfigHealPercent.Value}%");
            if (PhotonNetwork.IsMasterClient)
            {
                var props = PhotonNetwork.CurrentRoom.CustomProperties;
                props["HealPercent"] = (int)TruckHealPercent.ConfigHealPercent.Value;
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
    }
}
