using UnityEngine;

namespace RTS_FPS_V2.Networking
{
    public interface INetworkTransformSync
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        void SetSyncRateScale(float scale);
    }

    public interface INetworkStateSync
    {
        bool TryGetInt(string key, out int value);
        bool TryGetFloat(string key, out float value);
    }
}
