namespace PPM.Unsafe
{
    public record CoreInfo
    {
        private readonly List<CoreGroup> _associatedGroups = new();
        private bool _isSealed;

        public required uint Id { get; init; }

        public IReadOnlyList<CoreGroup> AssociatedGroups => _associatedGroups;

        public void AddAssociatedGroup(CoreGroup group)
        {
            if (_isSealed)
            {
                throw new InvalidOperationException("CoreInfo is sealed");
            }

            _associatedGroups.Add(group);
        }

        public void Seal()
        {
            _associatedGroups.TrimExcess();
            _isSealed = true;
        }
    }
}
