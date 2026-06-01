using System;

namespace RTS_FPS_V2.Networking
{
    public readonly struct PlayerConnectionEventArgs
    {
        public PlayerConnectionEventArgs(ulong clientId, int playerIndex)
        {
            ClientId = clientId;
            PlayerIndex = playerIndex;
        }

        public ulong ClientId { get; }
        public int PlayerIndex { get; }
    }

    public readonly struct SessionErrorEventArgs
    {
        public SessionErrorEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public interface INetworkSession
    {
        bool IsSessionActive { get; }
        bool IsServer { get; }
        bool IsClient { get; }
        bool IsHost { get; }
        ulong LocalClientId { get; }
        SessionConfig Config { get; }

        event Action SessionStarted;
        event Action SessionShutdown;
        event Action<PlayerConnectionEventArgs> PlayerJoined;
        event Action<PlayerConnectionEventArgs> PlayerLeft;
        event Action<SessionErrorEventArgs> ConnectionFailed;
        event Action<SessionErrorEventArgs> ConnectionRejected;

        bool StartHost();
        bool StartClient(ConnectionEndpoint endpoint);
        void Shutdown();
    }
}
