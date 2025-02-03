namespace PPM.Unsafe
{
    public record CoreGroup
    {
        public uint Id { get; init; }
    }

    public record PhysicalCoreGroup : CoreGroup
    {
    }

    public record CacheCoreGroup : CoreGroup, IComparable<CacheCoreGroup>
    {
        public byte Level { get; init; }
        public uint CacheSizeInB { get; init; }
        public int CompareTo(CacheCoreGroup? other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }
    }

    public record PerformanceCoreGroup : CoreGroup, IComparable<PerformanceCoreGroup>
    {
        public int CompareTo(PerformanceCoreGroup? other)
        {
            // Performance groups are sorted in descending order (from most performant to less performant).
            if (other is null) return 1;
            return -1 * Id.CompareTo(other.Id);
        }
    }
}
