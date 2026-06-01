using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NetworkEntitySpawner : MonoBehaviour, INetworkEntitySpawner
    {
        [SerializeField] NetworkEntityRegistry registry;

        readonly Dictionary<ulong, NetworkEntityHandle> entitiesById = new Dictionary<ulong, NetworkEntityHandle>();

        public event Action<EntitySpawnedEventArgs> EntitySpawned;
        public event Action<EntityDespawnedEventArgs> EntityDespawned;

        public static NetworkEntitySpawner Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public INetworkEntity Spawn(string prefabKey, Vector3 position, Quaternion rotation, ulong ownerClientId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("Only the server can spawn networked entities.");
                return null;
            }

            if (registry == null || !registry.TryGetPrefab(prefabKey, out var prefab))
            {
                Debug.LogError($"Unknown prefab key: {prefabKey}");
                return null;
            }

            var instance = Instantiate(prefab, position, rotation);
            var networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError($"Prefab '{prefabKey}' is missing NetworkObject.");
                Destroy(instance);
                return null;
            }

            var handle = instance.GetComponent<NetworkEntityHandle>();
            if (handle == null)
            {
                handle = instance.AddComponent<NetworkEntityHandle>();
            }

            if (ownerClientId != 0)
            {
                networkObject.SpawnWithOwnership(ownerClientId);
            }
            else
            {
                networkObject.Spawn();
            }

            entitiesById[networkObject.NetworkObjectId] = handle;
            EntitySpawned?.Invoke(new EntitySpawnedEventArgs(handle.Id, prefabKey, ownerClientId));
            return handle;
        }

        public void Despawn(NetworkEntityId entityId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("Only the server can despawn networked entities.");
                return;
            }

            if (!entityId.IsValid || !entitiesById.TryGetValue(entityId.Value, out var handle))
            {
                return;
            }

            entitiesById.Remove(entityId.Value);
            EntityDespawned?.Invoke(new EntityDespawnedEventArgs(entityId));

            if (handle != null && handle.NetworkObject != null && handle.NetworkObject.IsSpawned)
            {
                handle.NetworkObject.Despawn();
            }
        }

        public INetworkEntity TryGetEntity(NetworkEntityId entityId)
        {
            if (!entityId.IsValid)
            {
                return null;
            }

            return entitiesById.TryGetValue(entityId.Value, out var handle) ? handle : null;
        }

        public void CleanupEntitiesOwnedBy(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var toDespawn = new List<NetworkEntityId>();
            foreach (var pair in entitiesById)
            {
                if (pair.Value.OwnerClientId == clientId)
                {
                    toDespawn.Add(new NetworkEntityId(pair.Key));
                }
            }

            foreach (var entityId in toDespawn)
            {
                Despawn(entityId);
            }
        }

        public IEnumerable<NetworkEntityHandle> GetAllEntities() => entitiesById.Values;
    }
}
