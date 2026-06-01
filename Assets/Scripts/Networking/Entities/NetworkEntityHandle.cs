using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    public sealed class NetworkEntityHandle : NetworkBehaviour, INetworkEntity
    {
        NetworkEntityMetadata metadata;

        public NetworkEntityId Id => IsSpawned ? new NetworkEntityId(NetworkObjectId) : default;
        public EntityCategory Category => metadata != null ? metadata.Category : EntityCategory.Environment;
        public ulong OwnerClientId => NetworkObject != null ? NetworkObject.OwnerClientId : 0;
        public GameObject GameObject => gameObject;
        public bool IsSpawned => NetworkObject != null && NetworkObject.IsSpawned;

        void Awake()
        {
            metadata = GetComponent<NetworkEntityMetadata>();
        }
    }
}
