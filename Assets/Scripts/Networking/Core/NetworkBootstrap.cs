using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] NetworkSessionManager sessionManager;
        [SerializeField] NgoUnityTransportAdapter transport;
        [SerializeField] NetworkEntitySpawner entitySpawner;
        [SerializeField] NetworkRoleManager roleManager;
        [SerializeField] InterestManager interestManager;

        public INetworkSession Session => sessionManager;
        public INetworkTransport Transport => transport;
        public INetworkEntitySpawner EntitySpawner => entitySpawner;

        void Awake()
        {
            if (sessionManager == null)
            {
                sessionManager = GetComponent<NetworkSessionManager>();
            }

            if (entitySpawner == null)
            {
                entitySpawner = GetComponent<NetworkEntitySpawner>();
            }

            WirePlayerLeaveCleanup();
        }

        void WirePlayerLeaveCleanup()
        {
            if (sessionManager == null || entitySpawner == null)
            {
                return;
            }

            sessionManager.PlayerLeft += args =>
            {
                if (entitySpawner is NetworkEntitySpawner spawner)
                {
                    spawner.CleanupEntitiesOwnedBy(args.ClientId);
                }
            };
        }
    }
}
