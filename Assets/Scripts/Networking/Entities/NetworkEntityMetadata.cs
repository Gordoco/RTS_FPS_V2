using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    public sealed class NetworkEntityMetadata : MonoBehaviour
    {
        [SerializeField] EntityCategory category = EntityCategory.Environment;

        public EntityCategory Category => category;
    }
}
