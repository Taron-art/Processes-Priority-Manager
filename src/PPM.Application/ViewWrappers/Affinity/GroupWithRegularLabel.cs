using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers.Affinity
{
    public class GroupWithRegularLabel<T> : CoreGroupView<T> where T : CoreGroup
    {
        public override string Label
        {
            get
            {
                return string.Format(Strings.PPM.GroupWithRegularLabel, CoreGroup.Id);
            }
        }
    }
}
