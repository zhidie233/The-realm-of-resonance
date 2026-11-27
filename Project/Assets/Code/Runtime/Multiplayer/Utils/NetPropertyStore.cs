using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
namespace Resonance.Multiplayer
{

    /// <summary>
    /// Typed wrapper around Photon room/player custom properties.
    /// </summary>
    public static class NetPropertyStore
    {
        public static Hashtable CreateBag() => new Hashtable();

        public static void SetRoomProperty<T>(string key, T value)
        {
            if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null) return;
            var props = new Hashtable { { key, value } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        public static bool TryGetRoomProperty<T>(string key, out T value)
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object raw) &&
                raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public static void SetPlayerProperty<T>(Player player, string key, T value)
        {
            if (player == null) return;
            var props = new Hashtable { { key, value } };
            player.SetCustomProperties(props);
        }

        public static bool TryGetPlayerProperty<T>(Player player, string key, out T value)
        {
            if (player != null &&
                player.CustomProperties.TryGetValue(key, out object raw) &&
                raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }
    }
}
