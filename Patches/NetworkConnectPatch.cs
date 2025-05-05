using HarmonyLib;
using Photon.Pun;

// @kossnikita add sync host
namespace TruckHealPercent.Patches
{
    [HarmonyPatch(typeof(NetworkConnect), "TryJoiningRoom")]
    class NetworkConnectPatch
    {
        private static void Prefix()
        {
            PhotonNetwork.AddCallbackTarget(TruckHealPercent.configManager);
        }
    }
}
