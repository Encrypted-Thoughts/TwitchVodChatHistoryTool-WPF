using ChatHistory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TwitchVodChatHistoryTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChatToolContext context = new ChatToolContext();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = context;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ChatHistoryHelper = new ChatHistoryLogic(accessToken: context.AccessToken);
            if (VideosListBox.SelectedItem is Video selected)
                context.Comments = ChatHistoryHelper.GetVideoComments(selected.Id);
        }

        private void GetVideosButton_Click(object sender, RoutedEventArgs e)
        {
            var ChatHistoryHelper = new ChatHistoryLogic(accessToken: context.AccessToken);
            var videos = ChatHistoryHelper.GetVideos(new() { context.Username.Trim() });
            context.Videos = videos;
        }

        private void GetAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Replace twitchapps.com/tokengen with my own redirect site to show access token
            Process.Start(new ProcessStartInfo("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=icyqwwpy744ugu5x4ymyt6jqrnpxso&redirect_uri=https://twitchapps.com/tokengen&scope=chat:read&force_verify=true") { UseShellExecute = true });
        }

        public class ChatToolContext : INotifyPropertyChanged
        {
            private string m_username = "";
            private List<Video> m_videos = new();
            private List<Video> m_filteredVideos = new();
            private string m_videoTitleFilter = "";
            private DateTime? m_videoStartFilter;
            private DateTime? m_videoEndFilter;
            private List<SimplifiedComment> m_comments = new();
            private List<SimplifiedComment> m_filteredComments = new();
            private string m_commentUsernameFilter = "";
            private string m_commentMessageFilter = "";
            private string m_accessToken = "";
            private bool m_accessTokenSet = false;

            public string Username
            {
                get => m_username;
                set
                {
                    m_username = value;
                    NotifyPropertyChanged("Username");
                }
            }

            public List<Video> Videos
            {
                get => m_videos;
                set
                {
                    m_videos = value;
                    FilterVideos();
                    NotifyPropertyChanged("Videos");
                }
            }

            public List<Video> FilteredVideos
            {
                get => m_filteredVideos;
                set
                {
                    m_filteredVideos = value;
                    NotifyPropertyChanged("FilteredVideos");
                }
            }

            public string VideoTitleFilter
            {
                get => m_videoTitleFilter;
                set
                {
                    m_videoTitleFilter = value;
                    FilterVideos();
                    NotifyPropertyChanged("VideoTitleFilter");
                }
            }

            public DateTime? VideoStartFilter
            {
                get => m_videoStartFilter;
                set
                {
                    m_videoStartFilter = value;
                    FilterVideos();
                    NotifyPropertyChanged("VideoStartFilter");
                }
            }

            public DateTime? VideoEndFilter
            {
                get => m_videoEndFilter;
                set
                {
                    m_videoEndFilter = value;
                    FilterVideos();
                    NotifyPropertyChanged("VideoEndFilter");
                }
            }

            public List<SimplifiedComment> Comments
            {
                get => m_comments;
                set
                {
                    m_comments = value;
                    FilterComments();
                    NotifyPropertyChanged("Comments");
                }
            }

            public List<SimplifiedComment> FilteredComments
            {
                get => m_filteredComments;
                set
                {
                    m_filteredComments = value;
                    NotifyPropertyChanged("FilteredComments");
                }
            }

            public string CommentUsernameFilter
            {
                get => m_commentUsernameFilter;
                set
                {
                    m_commentUsernameFilter = value;
                    FilterComments();
                    NotifyPropertyChanged("CommentUsernameFilter");
                }
            }

            public string CommentMessageFilter
            {
                get => m_commentMessageFilter;
                set
                {
                    m_commentMessageFilter = value;
                    FilterComments();
                    NotifyPropertyChanged("CommentMessageFilter");
                }
            }

            public string AccessToken
            {
                get => m_accessToken;
                set
                {
                    m_accessToken = value;
                    AccessTokenSet = !string.IsNullOrWhiteSpace(m_accessToken);
                    NotifyPropertyChanged("AccessToken");
                }
            }

            public bool AccessTokenSet
            {
                get => m_accessTokenSet;
                set
                {
                    m_accessTokenSet = value;
                    NotifyPropertyChanged("AccessTokenSet");
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void NotifyPropertyChanged(string Info)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Info));
            }

            private void FilterVideos()
            {
                FilteredVideos = Videos.Where(v => FilterVideo(v)).ToList();
            }

            private bool FilterVideo(Video video)
            {
                var include = true;
                if (!string.IsNullOrWhiteSpace(VideoTitleFilter))
                    include = video.Title.Contains(VideoTitleFilter, StringComparison.OrdinalIgnoreCase);
                if (include && VideoStartFilter != null)
                    include = video.CreationDate >= VideoStartFilter;
                if (include && VideoEndFilter != null)
                    include = video.CreationDate <= VideoEndFilter;
                return include;
            }

            private void FilterComments()
            {
                FilteredComments = Comments.Where(c => FilterComment(c)).ToList();
            }

            private bool FilterComment(SimplifiedComment comment)
            {
                var include = true;
                if (!string.IsNullOrWhiteSpace(CommentUsernameFilter))
                    include = comment.Username.Contains(CommentUsernameFilter, StringComparison.OrdinalIgnoreCase);
                if (include && !string.IsNullOrWhiteSpace(CommentMessageFilter))
                    include = comment.Message.Contains(CommentMessageFilter, StringComparison.OrdinalIgnoreCase);
                return include;
            }
        }
    }
}
