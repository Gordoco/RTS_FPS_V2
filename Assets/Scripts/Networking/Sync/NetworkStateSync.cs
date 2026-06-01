using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NetworkStateSync : NetworkBehaviour, INetworkStateSync
    {
        readonly NetworkVariable<int> health = new NetworkVariable<int>(
            100,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        readonly NetworkVariable<int> orderState = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public bool TryGetInt(string key, out int value)
        {
            switch (key)
            {
                case "health":
                    value = health.Value;
                    return true;
                case "orderState":
                    value = orderState.Value;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }

        public bool TryGetFloat(string key, out float value)
        {
            value = default;
            return false;
        }

        public void SetHealth(int newHealth)
        {
            if (!IsServer)
            {
                return;
            }

            health.Value = newHealth;
        }

        public void SetOrderState(int newState)
        {
            if (!IsServer)
            {
                return;
            }

            orderState.Value = newState;
        }
    }
}
