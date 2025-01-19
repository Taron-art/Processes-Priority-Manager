using System;
using System.Threading;
using System.Threading.Tasks;
using Affinity_manager.Model.DataGathering;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Affinity_manager.ViewWrappers
{
    public sealed partial class ProcessInfoView : ObservableObject, IDisposable
    {
        private Task<BitmapImage?>? _imageTask;

        public ProcessInfoView(ProcessInfo processInfo, IApplicationIconsLoader iconLoader)
        {
            ProcessInfo = processInfo;
            IconLoader = iconLoader;
            processInfo.PropertyChanged += ProcessInfo_PropertyChanged;
        }

        private void ProcessInfo_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ProcessInfo.FriendlyName):
                    OnPropertyChanged(nameof(FriendlyName));
                    break;
                case nameof(ProcessInfo.IconSourcePath):
                    _imageTask = null;
                    OnPropertyChanged(nameof(ApplicationIcon));
                    break;
            }
        }

        public ProcessInfo ProcessInfo { get; }
        public IApplicationIconsLoader IconLoader { get; }

        public string MainModuleName { get => ProcessInfo.MainModuleName; }

        public string? FriendlyName { get => ProcessInfo.FriendlyName; }

        public BitmapImage? ApplicationIcon
        {
            get
            {
                Task<BitmapImage?>? fetchTask = null;
                fetchTask = _imageTask;
                if (fetchTask == null)
                {
                    fetchTask = IconLoader.LoadApplicationIconAsync(ProcessInfo.IconSourcePath);
                    fetchTask.ContinueWith(
                        t =>
                        {
                            // When the loading is done, notify the world that we loaded the icon.
                            OnPropertyChanged(nameof(ApplicationIcon));
                        }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
                    _imageTask = fetchTask;
                }

                return fetchTask.Status == TaskStatus.RanToCompletion ? fetchTask.Result : null;
            }
        }

        public override string ToString()
        {
            return MainModuleName;
        }

        public void Dispose()
        {
            ProcessInfo.PropertyChanged -= ProcessInfo_PropertyChanged;
            Task<BitmapImage?>? imageTask = _imageTask;
            _imageTask = null;
            imageTask?.Dispose();
        }
    }
}
