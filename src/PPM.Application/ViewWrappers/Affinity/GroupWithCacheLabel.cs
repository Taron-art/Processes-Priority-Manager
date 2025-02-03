using Affinity_manager.Utils;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers.Affinity
{
    public class GroupWithCacheLabel : CoreGroupView<CacheCoreGroup>
    {
        public override string Label
        {
            get
            {
                return string.Format(Strings.PPM.GroupWithCacheLabel, CoreGroup.Id, SizeFormatHelper.SizeToString(CoreGroup.CacheSizeInB));
            }
        }
    }
}
