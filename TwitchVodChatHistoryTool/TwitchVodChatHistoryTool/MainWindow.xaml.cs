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
        public ChatToolContext context = new();
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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = context;

            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
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
            //TODO: Replace twitchapps.com/tokengen with my own redirect site to show access token
            Process.Start(new ProcessStartInfo("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=icyqwwpy744ugu5x4ymyt6jqrnpxso&redirect_uri=https://twitchapps.com/tokengen&scope=chat:read&force_verify=true") { UseShellExecute = true });
        }

        private void DownloadJsonButton_Click(object sender, RoutedEventArgs e)
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

                var result = dlg.ShowDialog();

                if (result == true)
                {
                    var json = JsonSerializer.Serialize(context.FilteredComments);
                    var filename = dlg.FileName;

                    File.WriteAllText(filename, json);
                }
            }
        }

        private void DownloadCSVButton_Click(object sender, RoutedEventArgs e)
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

                var result = dlg.ShowDialog();

                if (result == true)
                {
                    var filename = dlg.FileName;
                    using TextWriter tw = new StreamWriter(filename);
                    tw.WriteLine("Timestamp,Username,Message");
                    foreach (var comment in context.FilteredComments)
                        tw.WriteLine($"{comment.Timestamp},{comment.Username},{comment.Message}");
                }
            }
        }
    }
}
