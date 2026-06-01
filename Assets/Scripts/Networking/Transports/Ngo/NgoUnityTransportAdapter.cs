using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NgoUnityTransportAdapter : MonoBehaviour, INetworkTransport
    {
        [SerializeField] NetworkManager networkManager;
        [SerializeField] UnityTransport unityTransport;

        public bool IsListening => networkManager != null && networkManager.IsListening;
        public bool IsConnected => networkManager != null && networkManager.IsConnectedClient;

        public event Action SessionStarted;
        public event Action SessionStopped;
        public event Action<TransportConnectionEventArgs> ClientConnectionChanged;
        public event Action<TransportErrorEventArgs> TransportError;

        void Awake()
        {
            if (networkManager == null)
            {
                networkManager = GetComponent<NetworkManager>();
            }

            if (unityTransport == null)
            {
                unityTransport = GetComponent<UnityTransport>();
            }
        }

        void OnEnable()
        {
            if (networkManager == null)
            {
                return;
            }

            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            networkManager.OnServerStarted += HandleServerStarted;
            networkManager.OnClientStarted += HandleClientStarted;
        }

        void OnDisable()
        {
            if (networkManager == null)
            {
                return;
            }

            networkManager.OnClientConnectedCallback -= HandleClientConnected;
            networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            networkManager.OnServerStarted -= HandleServerStarted;
            networkManager.OnClientStarted -= HandleClientStarted;
        }

        public bool StartHost(ConnectionEndpoint endpoint)
        {
            if (!TryConfigureDirectIp(endpoint, out var error))
            {
                TransportError?.Invoke(new TransportErrorEventArgs(error));
                return false;
            }

            if (networkManager.IsListening || networkManager.IsClient)
            {
                TransportError?.Invoke(new TransportErrorEventArgs("Network manager is already active."));
                return false;
            }

            return networkManager.StartHost();
        }

        public bool StartClient(ConnectionEndpoint endpoint)
        {
            if (!TryConfigureDirectIp(endpoint, out var error))
            {
                TransportError?.Invoke(new TransportErrorEventArgs(error));
                return false;
            }

            if (networkManager.IsListening || networkManager.IsClient)
            {
                TransportError?.Invoke(new TransportErrorEventArgs("Network manager is already active."));
                return false;
            }

            return networkManager.StartClient();
        }

        public void Shutdown()
        {
            if (networkManager != null && (networkManager.IsListening || networkManager.IsClient))
            {
                networkManager.Shutdown();
            }

            SessionStopped?.Invoke();
        }

        bool TryConfigureDirectIp(ConnectionEndpoint endpoint, out string error)
        {
            if (endpoint.TransportType == TransportType.SteamLobby)
            {
                error = "Steam lobby transport is not implemented in v1.";
                return false;
            }

            if (unityTransport == null)
            {
                error = "UnityTransport component is missing.";
                return false;
            }

            var address = string.IsNullOrWhiteSpace(endpoint.Address) ? "127.0.0.1" : endpoint.Address;
            unityTransport.SetConnectionData(address, endpoint.Port);
            error = null;
            return true;
        }

        void HandleServerStarted() => SessionStarted?.Invoke();

        void HandleClientStarted()
        {
            if (!networkManager.IsHost)
            {
                SessionStarted?.Invoke();
            }
        }

        void HandleClientConnected(ulong clientId) =>
            ClientConnectionChanged?.Invoke(new TransportConnectionEventArgs(clientId, true));

        void HandleClientDisconnected(ulong clientId) =>
            ClientConnectionChanged?.Invoke(new TransportConnectionEventArgs(clientId, false));
    }
}
