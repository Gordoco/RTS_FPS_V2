using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NetworkRoleManager : MonoBehaviour
    {
        readonly Dictionary<ulong, NetworkRole> rolesByClient = new Dictionary<ulong, NetworkRole>();

        public event Action<ulong, NetworkRole, NetworkRole> RoleChanged;

        public IRoleContext LocalRoleContext { get; private set; }

        public static NetworkRoleManager Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            LocalRoleContext = new RoleContext(NetworkRole.Spectator, 0);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void OnEnable()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            }
        }

        void OnDisable()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            }
        }

        public bool CanClientIssueCommand(ulong clientId, NetworkCommandType commandType)
        {
            if (!rolesByClient.TryGetValue(clientId, out var role))
            {
                return false;
            }

            return RoleAuthority.CanIssueCommand(role, commandType);
        }

        public bool TryGetRole(ulong clientId, out NetworkRole role) =>
            rolesByClient.TryGetValue(clientId, out role);

        public void AssignRole(ulong clientId, NetworkRole role)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            rolesByClient.TryGetValue(clientId, out var previousRole);
            rolesByClient[clientId] = role;
            BroadcastRole(clientId, role, previousRole);
        }

        void BroadcastRole(ulong clientId, NetworkRole role, NetworkRole previousRole)
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                LocalRoleContext = new RoleContext(role, clientId);
            }

            RoleChanged?.Invoke(clientId, previousRole, role);
        }

        void HandleClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var defaultRole = clientId == NetworkManager.ServerClientId
                ? NetworkRole.FpsOperator
                : NetworkRole.RtsCommander;
            AssignRole(clientId, defaultRole);
        }

        sealed class RoleContext : IRoleContext
        {
            public RoleContext(NetworkRole role, ulong clientId)
            {
                Role = role;
                ClientId = clientId;
            }

            public NetworkRole Role { get; }
            public ulong ClientId { get; }

            public bool CanIssueCommand(NetworkCommandType commandType) =>
                RoleAuthority.CanIssueCommand(Role, commandType);

            public bool IsEntityCategoryVisible(EntityCategory category) =>
                RoleAuthority.IsEntityCategoryVisible(Role, category);
        }
    }
}
