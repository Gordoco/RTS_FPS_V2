using System.Collections.Generic;

namespace RTS_FPS_V2.Networking
{
    public static class RoleAuthority
    {
        static readonly Dictionary<NetworkRole, HashSet<NetworkCommandType>> AllowedCommands =
            new Dictionary<NetworkRole, HashSet<NetworkCommandType>>
            {
                {
                    NetworkRole.FpsOperator,
                    new HashSet<NetworkCommandType>
                    {
                        NetworkCommandType.Movement,
                        NetworkCommandType.WeaponFire
                    }
                },
                {
                    NetworkRole.RtsCommander,
                    new HashSet<NetworkCommandType>
                    {
                        NetworkCommandType.UnitOrder,
                        NetworkCommandType.BuildCommand
                    }
                },
                {
                    NetworkRole.Spectator,
                    new HashSet<NetworkCommandType>()
                }
            };

        static readonly Dictionary<NetworkRole, HashSet<EntityCategory>> VisibleCategories =
            new Dictionary<NetworkRole, HashSet<EntityCategory>>
            {
                {
                    NetworkRole.FpsOperator,
                    new HashSet<EntityCategory>
                    {
                        EntityCategory.Character,
                        EntityCategory.Unit,
                        EntityCategory.Environment
                    }
                },
                {
                    NetworkRole.RtsCommander,
                    new HashSet<EntityCategory>
                    {
                        EntityCategory.Unit,
                        EntityCategory.Environment
                    }
                },
                {
                    NetworkRole.Spectator,
                    new HashSet<EntityCategory>
                    {
                        EntityCategory.Character,
                        EntityCategory.Unit,
                        EntityCategory.Environment
                    }
                }
            };

        public static bool CanIssueCommand(NetworkRole role, NetworkCommandType commandType) =>
            AllowedCommands.TryGetValue(role, out var commands) && commands.Contains(commandType);

        public static bool IsEntityCategoryVisible(NetworkRole role, EntityCategory category) =>
            VisibleCategories.TryGetValue(role, out var categories) && categories.Contains(category);
    }
}
