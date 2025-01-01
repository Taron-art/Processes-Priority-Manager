using System;
using Affinity_manager.ViewWrappers;
using Microsoft.UI.Xaml.Controls;

namespace Affinity_manager.Pages
{
    public sealed partial class AffinitySelectorDialog : ContentDialog
    {
        public AffinitySelectorDialog(AffinityView processAffinity)
        {
            ArgumentNullException.ThrowIfNull(processAffinity, nameof(processAffinity));
            ProcessAffinity = processAffinity;
            this.InitializeComponent();
        }

        public AffinityView ProcessAffinity
        {
            get;
        }
    }
}
