using ChatHistory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private ChatHistoryLogic ChatHistoryHelper = new();
        public ChatToolContext context = new ChatToolContext();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = context;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VideosListBox.SelectedItem is Video selected)
                context.Comments = ChatHistoryHelper.GetVideoComments(selected.Id);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var videos = ChatHistoryHelper.GetVideos(UsernamesText.Text.Split(',', StringSplitOptions.TrimEntries).ToList());
            context.Videos = videos;
        }

        public class ChatToolContext : INotifyPropertyChanged
        {
            private string m_usernames = "Usernames (separated by commas) Ex: encryptedthought,ninja,someotherusername";
            private List<Video> m_videos = new();
            private List<SimplifiedComment> m_comments = new();

            public string Usernames
            {
                get => m_usernames;
                set
                {
                    m_usernames = value;
                    NotifyPropertyChanged("Usernames");
                }
            }

            public List<Video> Videos
            {
                get => m_videos;
                set
                {
                    m_videos = value;
                    NotifyPropertyChanged("Videos");
                }
            }

            public List<SimplifiedComment> Comments
            {
                get => m_comments;
                set
                {
                    m_comments = value;
                    NotifyPropertyChanged("Comments");
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void NotifyPropertyChanged(string Info)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Info));
            }
        }
    }
}
