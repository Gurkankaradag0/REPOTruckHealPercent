using HarmonyLib;
using System.Reflection;
using UnityEngine;
using static PlayerHealth;

namespace TruckHealPercent.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar), "FinalHealRPC")]
    class PlayerAvatarPatch
    {
        private static readonly FieldInfo FinalHealField = typeof(PlayerAvatar).GetField("finalHeal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo IsLocalField = typeof(PlayerAvatar).GetField("isLocal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo PlayerNameField = typeof(PlayerAvatar).GetField("playerName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        [HarmonyPrefix]
        private static bool Prefix(PlayerAvatar __instance)
        {
            if (FinalHealField == null || IsLocalField == null || PlayerNameField == null)
            {
                TruckHealPercent.Logger.LogError("Failed to reflect fields in PlayerAvatar. Check assembly reference.");
                return true;
            }
            if ((bool)FinalHealField.GetValue(__instance))
            {
                return true;
            }
            if ((bool)IsLocalField.GetValue(__instance))
            {
                int value = TruckHealPercent.HealPercent.Value;
                object text = PlayerNameField.GetValue(__instance);
                int percentHealth = Mathf.RoundToInt(__instance.playerHealth.maxHealth * (value / 100f));
                TruckHealPercent.Logger.LogInfo($"Applying custom heal amount: {percentHealth} for player: {text}");
                __instance.playerHealth.EyeMaterialOverride((EyeOverrideState)2, 2f, 1);
                TruckScreenText.instance.MessageSendCustom("", text + " {arrowright}{truck}{check}\n {point}{shades}{pointright}<b><color=#00FF00>+" + percentHealth + "</color></b>{heart}", 0);
                __instance.playerHealth.Heal(percentHealth, true);
                FinalHealField.SetValue(__instance, true);
            }
            return false;
        }

        [HarmonyPostfix]
        private static void Postfix(PlayerAvatar __instance)
        {
            TruckHealer.instance.Heal(__instance);
            __instance.truckReturn.Play(__instance.PlayerVisionTarget.VisionTransform.position, 1f, 1f, 1f, 1f);
            __instance.truckReturnGlobal.Play(__instance.PlayerVisionTarget.VisionTransform.position, 1f, 1f, 1f, 1f);
            __instance.playerAvatarVisuals.effectGetIntoTruck.gameObject.SetActive(true);
        }
    }
}
