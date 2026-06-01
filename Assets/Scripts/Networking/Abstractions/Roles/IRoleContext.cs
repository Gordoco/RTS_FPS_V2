namespace RTS_FPS_V2.Networking
{
    public interface IRoleContext
    {
        NetworkRole Role { get; }
        ulong ClientId { get; }
        bool CanIssueCommand(NetworkCommandType commandType);
        bool IsEntityCategoryVisible(EntityCategory category);
    }
}
