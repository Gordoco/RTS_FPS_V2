using Unity.Netcode;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [DisallowMultipleComponent]
    public sealed class NetworkTransformSync : NetworkBehaviour, INetworkTransformSync
    {
        [SerializeField] float baseSyncInterval = 0.05f;

        float syncRateScale = 1f;
        float nextAllowedSyncTime;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        public void SetSyncRateScale(float scale)
        {
            syncRateScale = Mathf.Max(0.1f, scale);
        }

        void Update()
        {
            if (!IsServer || !IsSpawned)
            {
                return;
            }

            var interval = baseSyncInterval / syncRateScale;
            if (Time.time < nextAllowedSyncTime)
            {
                return;
            }

            nextAllowedSyncTime = Time.time + interval;
        }
    }
}
