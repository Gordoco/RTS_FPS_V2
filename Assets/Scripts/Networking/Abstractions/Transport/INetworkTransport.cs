using System;

namespace RTS_FPS_V2.Networking
{
    public readonly struct TransportConnectionEventArgs
    {
        public TransportConnectionEventArgs(ulong clientId, bool isConnected)
        {
            ClientId = clientId;
            IsConnected = isConnected;
        }

        public ulong ClientId { get; }
        public bool IsConnected { get; }
    }

    public readonly struct TransportErrorEventArgs
    {
        public TransportErrorEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public interface INetworkTransport
    {
        bool IsListening { get; }
        bool IsConnected { get; }

        event Action SessionStarted;
        event Action SessionStopped;
        event Action<TransportConnectionEventArgs> ClientConnectionChanged;
        event Action<TransportErrorEventArgs> TransportError;

        bool StartHost(ConnectionEndpoint endpoint);
        bool StartClient(ConnectionEndpoint endpoint);
        void Shutdown();
    }
}
