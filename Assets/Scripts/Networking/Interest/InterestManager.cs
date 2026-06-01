using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class InterestManager : MonoBehaviour
    {
        [SerializeField] NetworkSessionManager sessionManager;
        [SerializeField] int fullFidelityCellRadius = 2;
        [SerializeField] int dormantCellRadius = 5;

        readonly SpatialGrid spatialGrid = new SpatialGrid(10f);
        readonly Dictionary<ulong, Vector3> entityPositions = new Dictionary<ulong, Vector3>();
        readonly Dictionary<ulong, NetworkTransformSync> transformSyncByEntity = new Dictionary<ulong, NetworkTransformSync>();

        float nextTickTime;

        void Awake()
        {
            if (sessionManager == null)
            {
                sessionManager = FindFirstObjectByType<NetworkSessionManager>();
            }
        }

        void Update()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (sessionManager == null || sessionManager.Config == null)
            {
                return;
            }

            if (Time.time < nextTickTime)
            {
                return;
            }

            nextTickTime = Time.time + sessionManager.Config.InterestTickInterval;
            EvaluateInterest();
        }

        public void TrackEntity(NetworkEntityHandle entity)
        {
            if (entity == null || !entity.IsSpawned)
            {
                return;
            }

            var id = entity.Id.Value;
            var position = entity.transform.position;
            entityPositions[id] = position;
            spatialGrid.Insert(id, position);

            var sync = entity.GetComponent<NetworkTransformSync>();
            if (sync != null)
            {
                transformSyncByEntity[id] = sync;
            }
        }

        public void UntrackEntity(ulong entityId)
        {
            if (entityPositions.TryGetValue(entityId, out var position))
            {
                spatialGrid.Remove(entityId, position);
                entityPositions.Remove(entityId);
            }

            transformSyncByEntity.Remove(entityId);
        }

        void EvaluateInterest()
        {
            var spawner = NetworkEntitySpawner.Instance;
            var roleManager = NetworkRoleManager.Instance;
            if (spawner == null || roleManager == null)
            {
                return;
            }

            var clientObservers = BuildClientObservers(roleManager);

            foreach (var entity in spawner.GetAllEntities())
            {
                if (entity == null || !entity.IsSpawned)
                {
                    continue;
                }

                var entityId = entity.Id.Value;
                var position = entity.transform.position;
                if (entityPositions.TryGetValue(entityId, out var oldPosition))
                {
                    spatialGrid.Update(entityId, oldPosition, position);
                }
                else
                {
                    TrackEntity(entity);
                }

                entityPositions[entityId] = position;
                transformSyncByEntity.TryGetValue(entityId, out var transformSync);

                foreach (var pair in clientObservers)
                {
                    var clientId = pair.Key;
                    var observerContext = pair.Value;
                    var isRoleRelevant = observerContext.IsEntityCategoryVisible(entity.Category);
                    var isSpatiallyNear = spatialGrid.IsRelevant(position, observerContext.ObserverPosition, fullFidelityCellRadius);
                    var isSpatiallyDistant = spatialGrid.IsRelevant(position, observerContext.ObserverPosition, dormantCellRadius);

                    var shouldShow = isRoleRelevant && isSpatiallyDistant;
                    var networkObject = entity.NetworkObject;
                    if (networkObject == null)
                    {
                        continue;
                    }

                    if (shouldShow && !networkObject.IsNetworkVisibleTo(clientId))
                    {
                        networkObject.NetworkShow(clientId);
                    }
                    else if (!shouldShow && networkObject.IsNetworkVisibleTo(clientId))
                    {
                        networkObject.NetworkHide(clientId);
                    }

                    if (transformSync != null)
                    {
                        if (isRoleRelevant && isSpatiallyNear)
                        {
                            transformSync.SetSyncRateScale(1f);
                        }
                        else if (isRoleRelevant && isSpatiallyDistant)
                        {
                            transformSync.SetSyncRateScale(0.25f);
                        }
                        else
                        {
                            transformSync.SetSyncRateScale(0.1f);
                        }
                    }
                }
            }
        }

        Dictionary<ulong, ObserverContext> BuildClientObservers(NetworkRoleManager roleManager)
        {
            var result = new Dictionary<ulong, ObserverContext>();
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!roleManager.TryGetRole(clientId, out var role))
                {
                    role = NetworkRole.Spectator;
                }

                var observerPosition = Vector3.zero;
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)
                    && client.PlayerObject != null)
                {
                    observerPosition = client.PlayerObject.transform.position;
                }

                result[clientId] = new ObserverContext(role, observerPosition);
            }

            return result;
        }

        readonly struct ObserverContext
        {
            public ObserverContext(NetworkRole role, Vector3 observerPosition)
            {
                Role = role;
                ObserverPosition = observerPosition;
            }

            public NetworkRole Role { get; }
            public Vector3 ObserverPosition { get; }

            public bool IsEntityCategoryVisible(EntityCategory category) =>
                RoleAuthority.IsEntityCategoryVisible(Role, category);
        }
    }
}
