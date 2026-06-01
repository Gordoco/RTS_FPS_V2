using System;

namespace RTS_FPS_V2.Networking
{
    [Serializable]
    public struct ConnectionEndpoint
    {
        public TransportType TransportType;

        public string Address;
        public ushort Port;

        /// <summary>Reserved for future Steam lobby / relay join.</summary>
        public ulong SteamLobbyId;

        public static ConnectionEndpoint DirectIp(string address, ushort port) =>
            new ConnectionEndpoint
            {
                TransportType = TransportType.DirectIp,
                Address = address,
                Port = port,
                SteamLobbyId = 0
            };

        public static ConnectionEndpoint SteamLobby(ulong lobbyId) =>
            new ConnectionEndpoint
            {
                TransportType = TransportType.SteamLobby,
                Address = string.Empty,
                Port = 0,
                SteamLobbyId = lobbyId
            };
    }
}
