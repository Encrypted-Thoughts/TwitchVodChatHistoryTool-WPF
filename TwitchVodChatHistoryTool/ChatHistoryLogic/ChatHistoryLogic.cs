using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using TwitchLib.Api;

namespace ChatHistory
{
    public class ChatHistoryLogic
    {
        private readonly TwitchAPI Api = new();
        public string ClientId { get; set; }

        public ChatHistoryLogic(string cliendId = "qbij4eq1e5q9pce0lqp7qxh94vwk2g")
        {
            Api.Settings.ClientId = cliendId;
            ClientId = cliendId;
        }

        public List<Video> GetVideos(List<string> users)
        {
            var history = new List<Video>();

            foreach (var user in users)
            {
                var userInfo = Api.V5.Users.GetUserByNameAsync(user).Result;
                var videos = Api.V5.Channels.GetChannelVideosAsync(userInfo.Matches.First().Id, limit: 100).Result;
                foreach (var video in videos.Videos)
                {
                    var newVideo = new Video
                    {
                        Id = video.Id.TrimStart('v'),
                        Title = video.Title
                    };
                    history.Add(newVideo);
                }
            }
            return history;
        }

        public List<Video> GetChatHistoryByVideo(List<string> users)
        {
            var history = new List<Video>();

            foreach (var user in users)
            {
                var userInfo = Api.V5.Users.GetUserByNameAsync(user).Result;
                var videos = Api.V5.Channels.GetChannelVideosAsync(userInfo.Matches.First().Id, limit: 0).Result;
                foreach (var video in videos.Videos)
                {
                    var newVideo = new Video
                    {
                        Id = video.Id.TrimStart('v'),
                        Title = video.Title
                    };
                    newVideo.Comments = GetVideoComments(newVideo.Id);
                    history.Add(newVideo);
                }
            }

            return history;
        }

        public List<SimplifiedComment> GetVideoComments(string id)
        {
            var comments = new List<SimplifiedComment>();
            var baseUrl = $"{"https"}://api.twitch.tv/v5/videos/{id}/comments";
            string? nextCursor = null;
            do
            {
                string url = nextCursor == null ?
                    $"{baseUrl}?content_offset_seconds=0" :
                    $"{baseUrl}?cursor={nextCursor}";

                var response = JObject.Parse(TwitchRequest(url)).ToObject<CommentContainer>();
                if (response != null)
                {
                    foreach(var comment in response.Comments)
                    {
                        var simplified = new SimplifiedComment
                        {
                            Timestamp = comment.CreatedAt,
                            Username = comment.Commenter.DisplayName,
                            Message = comment.Message.Body
                        };
                        comments.Add(simplified);
                    }
                    nextCursor = response.Next;
                }
                else
                    break;
            }
            while (nextCursor != null);
            return comments;
        }

        private string TwitchRequest(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Client-ID", ClientId);
            var response = client.GetAsync(url).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
    }

    public class Video
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<SimplifiedComment> Comments { get; set; }
    }

    public class SimplifiedComment
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
    }

    public class CommentContainer
    {
        [JsonProperty("comments")]
        public Comment[] Comments { get; set; }
        [JsonProperty("_next")]
        public string Next { get; set; }
    }

    public class Comment
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }
        [JsonProperty("content_type")]
        public string ContentType { get; set; }
        [JsonProperty("content_id")]
        public string ContentId { get; set; }
        [JsonProperty("content_offset_seconds")]
        public float ContentOffsetSeconds { get; set; }
        [JsonProperty("commenter")]
        public Commenter Commenter { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("message")]
        public Message Message { get; set; }
    }

    public class Commenter
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("bio")]
        public string Bio { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("logo")]
        public string Logo { get; set; }
    }

    public class Message
    {
        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("is_action")]
        public bool IsAction { get; set; }
        [JsonProperty("user_badges")]
        public User_Badges[] UserBadges { get; set; }
        [JsonProperty("user_color")]
        public string UserColor { get; set; }
    }

    public class User_Badges
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }

}