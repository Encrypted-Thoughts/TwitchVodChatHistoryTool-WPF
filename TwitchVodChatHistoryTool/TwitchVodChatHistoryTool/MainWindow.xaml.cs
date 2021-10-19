using ChatHistory;
using ChatHistory.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace TwitchVodChatHistoryTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ChatToolContext context = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = context;
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

        private void Worker_GetVideos(object sender, DoWorkEventArgs e)
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

        private void Worker_GetComments(object sender, DoWorkEventArgs e)
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

        private void Worker_HideProgressBar(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.IsIndeterminate = false;
        }

        private void GetAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Replace twitchapps.com/tokengen with my own redirect site to show access token
            Process.Start(new ProcessStartInfo("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=icyqwwpy744ugu5x4ymyt6jqrnpxso&redirect_uri=https://twitchapps.com/tokengen&scope=chat:read&force_verify=true") { UseShellExecute = true });
        }
    }
}
