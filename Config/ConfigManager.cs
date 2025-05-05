using ExitGames.Client.Photon;
using Photon.Pun;
using System;

// @kossnikita add sync host
namespace TruckHealPercent.Config
{
    public class ConfigManager : MonoBehaviourPunCallbacks
    {
        public override void OnCreatedRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var props = PhotonNetwork.CurrentRoom.CustomProperties;
                props["HealPercent"] = TruckHealPercent.HealPercent;
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
            TruckHealPercent.Logger.LogDebug("Room properties updated");
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
                props["HealPercent"] = TruckHealPercent.ConfigHealPercent.Value;
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
    }
}
