using ChatHistory;
using ChatHistory.Models;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace TwitchVodChatHistoryTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Windows 11 Corner Rounding
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long DwmSetWindowAttribute(IntPtr hwnd,
                                                         DWMWINDOWATTRIBUTE attribute,
                                                         ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                         uint cbAttribute);
        #endregion

        public ChatToolContext context = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = context;

            #region Windows 11 Corner Rounding
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            #endregion
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VideosListBox.SelectedItem is Video video)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += Worker_GetComments;
                worker.RunWorkerCompleted += Worker_HideProgressBar;

                ProgressBar.IsIndeterminate = true;
                worker.RunWorkerAsync(argument: video);
            }
        }

        private void GetVideosButton_Click(object sender, RoutedEventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += Worker_GetVideos;
            worker.RunWorkerCompleted += Worker_HideProgressBar;

            ProgressBar.IsIndeterminate = true;
            worker.RunWorkerAsync();
        }

        private void Worker_GetVideos(object? sender, DoWorkEventArgs e)
        {
            try
            {
                var ChatHistoryHelper = new ChatHistoryLogic(accessToken: context.AccessToken);
                var videos = ChatHistoryHelper.GetVideos(new() { context.Username.Trim() });
                context.Videos = videos;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Worker_GetComments(object? sender, DoWorkEventArgs e)
        {
            try
            {
                if (e.Argument is Video video)
                {
                    var ChatHistoryHelper = new ChatHistoryLogic(accessToken: context.AccessToken);
                    context.Comments = ChatHistoryHelper.GetVideoComments(video.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Worker_HideProgressBar(object? sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.IsIndeterminate = false;
        }

        private void GetAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=icyqwwpy744ugu5x4ymyt6jqrnpxso&redirect_uri=https://encrypted-thoughts.github.io/auth&scope=chat:read&force_verify=true") { UseShellExecute = true });
        }

        private void DownloadJsonButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VideosListBox.SelectedItem is Video video)
                {
                    var invalids = Path.GetInvalidFileNameChars();
                    var title = string.Join("_", video.Title.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

                    var dlg = new SaveFileDialog
                    {
                        FileName = title,
                        DefaultExt = ".json",
                        Filter = "Json files (*.json)|*.json"
                    };

                    if (dlg.ShowDialog() == true)
                        File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(context.FilteredComments));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DownloadCSVButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VideosListBox.SelectedItem is Video video)
                {
                    var invalids = Path.GetInvalidFileNameChars();
                    var title = string.Join("_", video.Title.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

                    var dlg = new SaveFileDialog
                    {
                        FileName = title,
                        DefaultExt = ".csv",
                        Filter = "CSV files (*.csv)|*.csv"
                    };

                    if (dlg.ShowDialog() == true)
                    {
                        using TextWriter tw = new StreamWriter(dlg.FileName);
                        tw.WriteLine("Timestamp,Username,Message");
                        foreach (var comment in context.FilteredComments)
                            tw.WriteLine($"{comment.Timestamp},{comment.Username},{comment.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
