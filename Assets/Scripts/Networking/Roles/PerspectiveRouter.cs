using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    public sealed class PerspectiveRouter : MonoBehaviour
    {
        [SerializeField] GameObject fpsRig;
        [SerializeField] GameObject rtsRig;

        NetworkRoleManager roleManager;

        void Awake()
        {
            roleManager = FindFirstObjectByType<NetworkRoleManager>();
        }

        void OnEnable()
        {
            if (roleManager != null)
            {
                roleManager.RoleChanged += HandleRoleChanged;
                ApplyRole(roleManager.LocalRoleContext.Role);
            }
        }

        void OnDisable()
        {
            if (roleManager != null)
            {
                roleManager.RoleChanged -= HandleRoleChanged;
            }
        }

        void HandleRoleChanged(ulong clientId, NetworkRole previousRole, NetworkRole newRole)
        {
            if (roleManager != null && clientId == roleManager.LocalRoleContext.ClientId)
            {
                ApplyRole(newRole);
            }
        }

        void ApplyRole(NetworkRole role)
        {
            if (fpsRig != null)
            {
                fpsRig.SetActive(role == NetworkRole.FpsOperator);
            }

            if (rtsRig != null)
            {
                rtsRig.SetActive(role == NetworkRole.RtsCommander);
            }
        }
    }
}
