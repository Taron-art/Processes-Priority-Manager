using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Affinity_manager.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Vanara.PInvoke;
using Windows.ApplicationModel.DataTransfer;

namespace Affinity_manager.Exceptions
{
    public class UnhandledExceptionHandler : IHostedService
    {
        public UnhandledExceptionHandler(IOptions<ReportingSettings> reportingSettings)
        {
            Settings = reportingSettings.Value;
        }

        public ReportingSettings Settings { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        internal void AttachHandler(Application app)
        {
            app.UnhandledException += (sender, args) =>
            {
                Exception exception = args.Exception;
                string message = $"Unhandled exception: {exception.Message}";
                nint mainWindowHandle = nint.Zero;

                try
                {
                    DisplayErrorDialog(mainWindowHandle, exception);
                }
                catch
                {
                    // We don't want to go into the infinite loop of exceptions.
                }
                Environment.FailFast(message, exception);
            };
        }

        private void DisplayErrorDialog(nint parentHandle, Exception exception)
        {
            // We are not using localized string here to avoid potential point of failure.
            ComCtl32.TASKDIALOGCONFIG config = new()
            {
                dwCommonButtons = ComCtl32.TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_CLOSE_BUTTON,

                MainInstruction = "The Process Priority Manager has encountered an unexpected error and will be closed.",
                Content = "Please report this message to the developer by clicking the 'Report a problem' button. It will copy details to the clipboard and opens the issues page on the GitHub.",
                ExpandedControlText = "Details",
                ExpandedInformation = exception.ToString(),
                hwndParent = parentHandle,
            };

            ComCtl32.TASKDIALOG_BUTTON[] buttons =
            [
                new ComCtl32.TASKDIALOG_BUTTON() { nButtonID = 666, pszButtonText = Marshal.StringToHGlobalAuto("Report a problem") }
            ];

            config.cButtons = (uint)buttons.Length;
            config.pButtons = Marshal.UnsafeAddrOfPinnedArrayElement(buttons, 0);
            config.nDefaultButton = 8; // Close button
            config.mainIcon = (nint)ComCtl32.TaskDialogIcon.TD_ERROR_ICON;

            ComCtl32.TaskDialogIndirect(config, out int pnButton, out _, out _);
            if (pnButton == buttons[0].nButtonID && !string.IsNullOrWhiteSpace(Settings.IssueReportingUrl))
            {
                ReportIssue(exception, Settings.IssueReportingUrl);
            }
        }

        private void ReportIssue(Exception exception, string issueReportingUrl)
        {
            try
            {
                // Just check that we are not launching any application or file.
                Uri urlChecker = new(issueReportingUrl);
                if (urlChecker.IsFile || urlChecker.IsUnc)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            DataPackage dataPackage = new();
            dataPackage.SetText(GenerateExceptionReport(exception));

            Clipboard.SetContentWithOptions(dataPackage, new ClipboardContentOptions { IsAllowedInHistory = true });
            Clipboard.Flush();

            // An attempt to open the browser with the URL without elevated rights.
            ProcessStartInfo psi = new()
            {
                FileName = "explorer.exe",
                Arguments = issueReportingUrl,
                UseShellExecute = false
            };
            Process.Start(psi);
        }

        private static string GenerateExceptionReport(Exception exception)
        {
            StringBuilder sb = new();
            sb.AppendLine("### Exception details");
            sb.Append(exception.ToString());
            return sb.ToString();
        }
    }
}
