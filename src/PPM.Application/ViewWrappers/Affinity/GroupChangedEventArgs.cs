using System;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers.Affinity
{
    public class GroupChangedEventArgs<T> : EventArgs where T : CoreGroup
    {
        public GroupChangedEventArgs(CoreGroupView<T> coreGroupView)
        {
            CoreGroupView = coreGroupView;
        }
        public CoreGroupView<T> CoreGroupView { get; }
    }
}
