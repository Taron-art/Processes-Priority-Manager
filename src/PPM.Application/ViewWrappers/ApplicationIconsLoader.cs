using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Affinity_manager.ViewWrappers
{
    public class ApplicationIconsLoader : IApplicationIconsLoader
    {
        private const string defaultIconSource = "Ping.exe";
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private Task<BitmapImage?>? _defaultIconLoadTask;

        public async Task<BitmapImage?> LoadApplicationIconAsync(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    // For some reason when you allow async requests to stack-up,
                    // COM Exceptions starts to emerge. 
                    await _semaphore.WaitAsync();

                    StorageItemThumbnail iconThumbnail;
                    try
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                        iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 32, ThumbnailOptions.UseCurrentScale);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

                    BitmapImage image = new();
                    await image.SetSourceAsync(iconThumbnail);
                    return image;
                }
                catch (COMException)
                {
                    // Something did go wrong with image loading.
                    if (defaultIconSource.Equals(path, StringComparison.Ordinal))
                    {
                        return null;
                    }
                }
            }

            string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

            return await (_defaultIconLoadTask ??= LoadApplicationIconAsync(Path.Combine(system32, defaultIconSource)));
        }
    }
}
