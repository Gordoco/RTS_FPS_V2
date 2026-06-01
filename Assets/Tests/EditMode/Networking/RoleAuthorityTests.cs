using NUnit.Framework;
using RTS_FPS_V2.Networking;

namespace RTS_FPS_V2.Tests.EditMode.Networking
{
    public class RoleAuthorityTests
    {
        [Test]
        public void RtsCommander_CanIssueUnitOrders()
        {
            Assert.That(
                RoleAuthority.CanIssueCommand(NetworkRole.RtsCommander, NetworkCommandType.UnitOrder),
                Is.True);
        }

        [Test]
        public void RtsCommander_CannotIssueWeaponFire()
        {
            Assert.That(
                RoleAuthority.CanIssueCommand(NetworkRole.RtsCommander, NetworkCommandType.WeaponFire),
                Is.False);
        }

        [Test]
        public void FpsOperator_CanIssueMovement()
        {
            Assert.That(
                RoleAuthority.CanIssueCommand(NetworkRole.FpsOperator, NetworkCommandType.Movement),
                Is.True);
        }
    }
}
