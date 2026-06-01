namespace RTS_FPS_V2.Networking
{
    public readonly struct NetworkEntityId : System.IEquatable<NetworkEntityId>
    {
        public NetworkEntityId(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; }

        public bool IsValid => Value != 0;

        public bool Equals(NetworkEntityId other) => Value == other.Value;

        public override bool Equals(object obj) => obj is NetworkEntityId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();

        public static bool operator ==(NetworkEntityId left, NetworkEntityId right) => left.Equals(right);

        public static bool operator !=(NetworkEntityId left, NetworkEntityId right) => !left.Equals(right);
    }
}
