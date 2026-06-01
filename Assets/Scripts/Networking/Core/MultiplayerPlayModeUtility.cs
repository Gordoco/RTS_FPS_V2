namespace RTS_FPS_V2.Networking.Ngo
{
    /// <summary>
    /// Thin wrapper over Unity Multiplayer Play Mode so runtime assemblies avoid hard dependency when the package is absent.
    /// </summary>
    public static class MultiplayerPlayModeUtility
    {
        public static bool IsAvailable
        {
            get
            {
#if UNITY_EDITOR
                return GetIsMainEditorProperty() != null;
#else
                return false;
#endif
            }
        }

        public static bool IsMainEditorPlayer
        {
            get
            {
#if UNITY_EDITOR
                var property = GetIsMainEditorProperty();
                if (property == null)
                {
                    return true;
                }

                return (bool)property.GetValue(null);
#else
                return true;
#endif
            }
        }

#if UNITY_EDITOR
        static System.Reflection.PropertyInfo GetIsMainEditorProperty()
        {
            const string typeName = "Unity.Multiplayer.PlayMode.CurrentPlayer, Unity.Multiplayer.PlayMode";
            var playerType = System.Type.GetType(typeName);
            return playerType?.GetProperty("IsMainEditor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        }
#endif
    }
}
