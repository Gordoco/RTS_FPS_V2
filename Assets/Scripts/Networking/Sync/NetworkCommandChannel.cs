using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NetworkCommandChannel : NetworkBehaviour, INetworkCommandChannel
    {
        NetworkRoleManager roleManager;

        void Awake()
        {
            roleManager = FindFirstObjectByType<NetworkRoleManager>();
        }

        public bool CanSubmitCommand(NetworkCommandType commandType)
        {
            if (roleManager == null || !IsOwner)
            {
                return false;
            }

            return roleManager.LocalRoleContext.CanIssueCommand(commandType);
        }

        public void SubmitCommand(NetworkCommandType commandType, int payload = 0)
        {
            if (!CanSubmitCommand(commandType))
            {
                return;
            }

            SubmitCommandServerRpc((int)commandType, payload);
        }

        [ServerRpc]
        void SubmitCommandServerRpc(int commandTypeValue, int payload, ServerRpcParams rpcParams = default)
        {
            var commandType = (NetworkCommandType)commandTypeValue;
            if (roleManager == null || !roleManager.CanClientIssueCommand(rpcParams.Receive.SenderClientId, commandType))
            {
                return;
            }

            var stateSync = GetComponent<NetworkStateSync>();
            if (stateSync == null)
            {
                return;
            }

            switch (commandType)
            {
                case NetworkCommandType.Movement:
                case NetworkCommandType.WeaponFire:
                    stateSync.SetOrderState(payload);
                    break;
                case NetworkCommandType.UnitOrder:
                case NetworkCommandType.BuildCommand:
                    stateSync.SetOrderState(payload);
                    break;
            }
        }
    }
}
