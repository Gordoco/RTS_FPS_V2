using System;
using UnityEngine;

namespace RTS_FPS_V2.Networking
{
    public interface INetworkEntity
    {
        NetworkEntityId Id { get; }
        EntityCategory Category { get; }
        ulong OwnerClientId { get; }
        GameObject GameObject { get; }
        bool IsSpawned { get; }
    }

    public readonly struct EntitySpawnedEventArgs
    {
        public EntitySpawnedEventArgs(NetworkEntityId entityId, string prefabKey, ulong ownerClientId)
        {
            EntityId = entityId;
            PrefabKey = prefabKey;
            OwnerClientId = ownerClientId;
        }

        public NetworkEntityId EntityId { get; }
        public string PrefabKey { get; }
        public ulong OwnerClientId { get; }
    }

    public readonly struct EntityDespawnedEventArgs
    {
        public EntityDespawnedEventArgs(NetworkEntityId entityId)
        {
            EntityId = entityId;
        }

        public NetworkEntityId EntityId { get; }
    }

    public interface INetworkEntitySpawner
    {
        event Action<EntitySpawnedEventArgs> EntitySpawned;
        event Action<EntityDespawnedEventArgs> EntityDespawned;

        INetworkEntity Spawn(string prefabKey, Vector3 position, Quaternion rotation, ulong ownerClientId);
        void Despawn(NetworkEntityId entityId);
        INetworkEntity TryGetEntity(NetworkEntityId entityId);
    }
}
