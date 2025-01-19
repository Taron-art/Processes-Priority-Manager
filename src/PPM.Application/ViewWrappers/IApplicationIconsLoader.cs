using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Affinity_manager.ViewWrappers
{
    public interface IApplicationIconsLoader
    {
        Task<BitmapImage?> LoadApplicationIconAsync(string? path);
    }
}