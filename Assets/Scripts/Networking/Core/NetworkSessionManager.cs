using System;
using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NetworkSessionManager : MonoBehaviour, INetworkSession
    {
        [SerializeField] SessionConfig config;
        [SerializeField] NgoUnityTransportAdapter transport;
        [SerializeField] NetworkManager networkManager;

        public bool IsSessionActive => networkManager != null && (networkManager.IsListening || networkManager.IsClient);
        public bool IsServer => networkManager != null && networkManager.IsServer;
        public bool IsClient => networkManager != null && networkManager.IsClient;
        public bool IsHost => networkManager != null && networkManager.IsHost;
        public ulong LocalClientId => networkManager != null ? networkManager.LocalClientId : 0;
        public SessionConfig Config => config;

        public event Action SessionStarted;
        public event Action SessionShutdown;
        public event Action<PlayerConnectionEventArgs> PlayerJoined;
        public event Action<PlayerConnectionEventArgs> PlayerLeft;
        public event Action<SessionErrorEventArgs> ConnectionFailed;
        public event Action<SessionErrorEventArgs> ConnectionRejected;

        public static NetworkSessionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (networkManager == null)
            {
                networkManager = GetComponent<NetworkManager>();
            }

            if (transport == null)
            {
                transport = GetComponent<NgoUnityTransportAdapter>();
            }
        }

        void OnEnable()
        {
            if (transport != null)
            {
                transport.SessionStarted += HandleTransportSessionStarted;
                transport.SessionStopped += HandleTransportSessionStopped;
                transport.TransportError += HandleTransportError;
            }

            if (networkManager != null)
            {
                networkManager.NetworkConfig.ConnectionApproval = true;
                networkManager.ConnectionApprovalCallback = ApprovalCallback;
                networkManager.OnClientConnectedCallback += HandleClientConnected;
                networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        void OnDisable()
        {
            if (transport != null)
            {
                transport.SessionStarted -= HandleTransportSessionStarted;
                transport.SessionStopped -= HandleTransportSessionStopped;
                transport.TransportError -= HandleTransportError;
            }

            if (networkManager != null)
            {
                networkManager.ConnectionApprovalCallback = null;
                networkManager.OnClientConnectedCallback -= HandleClientConnected;
                networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool StartHost()
        {
            if (config == null)
            {
                ConnectionFailed?.Invoke(new SessionErrorEventArgs("SessionConfig is not assigned."));
                return false;
            }

            var endpoint = ConnectionEndpoint.DirectIp("0.0.0.0", config.Port);
            return transport != null && transport.StartHost(endpoint);
        }

        public bool StartClient(ConnectionEndpoint endpoint)
        {
            if (config == null)
            {
                ConnectionFailed?.Invoke(new SessionErrorEventArgs("SessionConfig is not assigned."));
                return false;
            }

            if (endpoint.Port == 0)
            {
                endpoint = ConnectionEndpoint.DirectIp(endpoint.Address, config.Port);
            }

            return transport != null && transport.StartClient(endpoint);
        }

        public void Shutdown()
        {
            transport?.Shutdown();
            SessionShutdown?.Invoke();
        }

        void ApprovalCallback(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            if (config != null && networkManager.ConnectedClients.Count >= config.MaxPlayers)
            {
                response.Approved = false;
                response.Reason = "Session is full.";
                ConnectionRejected?.Invoke(new SessionErrorEventArgs(response.Reason));
                return;
            }

            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        void HandleTransportSessionStarted() => SessionStarted?.Invoke();

        void HandleTransportSessionStopped() => SessionShutdown?.Invoke();

        void HandleTransportError(TransportErrorEventArgs args) =>
            ConnectionFailed?.Invoke(new SessionErrorEventArgs(args.Message));

        void HandleClientConnected(ulong clientId)
        {
            var playerIndex = 0;
            var ids = networkManager.ConnectedClientsIds;
            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] == clientId)
                {
                    playerIndex = i;
                    break;
                }
            }

            PlayerJoined?.Invoke(new PlayerConnectionEventArgs(clientId, playerIndex));
        }

        void HandleClientDisconnected(ulong clientId)
        {
            PlayerLeft?.Invoke(new PlayerConnectionEventArgs(clientId, -1));
        }
    }
}
