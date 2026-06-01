namespace RTS_FPS_V2.Networking
{
    public enum NetworkCommandType
    {
        Movement,
        UnitOrder,
        WeaponFire,
        BuildCommand
    }

    public interface INetworkCommandChannel
    {
        void SubmitCommand(NetworkCommandType commandType, int payload = 0);
        bool CanSubmitCommand(NetworkCommandType commandType);
    }
}
