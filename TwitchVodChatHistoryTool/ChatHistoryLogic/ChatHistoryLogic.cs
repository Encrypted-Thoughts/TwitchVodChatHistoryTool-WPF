using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using TwitchLib.Api;
using ChatHistory.Models;

namespace ChatHistory
{
    public class ChatHistoryLogic
    {
        public string ClientId { get; set; }
        public string AccessToken { get; set; }

        public ChatHistoryLogic(string accessToken = "", string cliendId = "icyqwwpy744ugu5x4ymyt6jqrnpxso")
        {
            ClientId = cliendId;
            AccessToken = accessToken;
        }

        public List<Video> GetVideos(List<string> users)
        {
            var history = new List<Video>();
            var Api = new TwitchAPI();
            Api.Settings.ClientId = ClientId;
            Api.Settings.AccessToken = AccessToken;
            var userList = Api.Helix.Users.GetUsersAsync(logins: users).Result;

            string? cursor = null;
            foreach (var user in userList.Users)
            {
                do
                {
                    var videos = Api.Helix.Videos.GetVideoAsync(userId: user.Id, first: 100, after: cursor).Result;
                    foreach (var video in videos.Videos)
                    {
                        var timestamp = DateTime.Parse(video.CreatedAt).ToLocalTime();
                        var newVideo = new Video
                        {
                            Id = video.Id,
                            Title = timestamp.ToShortDateString() + ": " + video.Title,
                            CreationDate = timestamp
                        };
                        history.Add(newVideo);
                    }
                    cursor = videos.Pagination.Cursor;
                } while (cursor != null);
            }

            return history;
        }

        public List<Video> GetChatHistoryByVideo(List<string> users)
        {
            var history = new List<Video>();
            var Api = new TwitchAPI();
            Api.Settings.ClientId = ClientId;
            Api.Settings.AccessToken = AccessToken;
            var userList = Api.Helix.Users.GetUsersAsync(logins: users).Result;

            string? cursor = null;
            foreach (var user in userList.Users)
            {
                do
                {
                    var videos = Api.Helix.Videos.GetVideoAsync(userId: user.Id, first: 100, after: cursor).Result;
                    foreach (var video in videos.Videos)
                    {
                        var timestamp = DateTime.Parse(video.CreatedAt).ToLocalTime();
                        var newVideo = new Video
                        {
                            Id = video.Id,
                            Title = timestamp.ToShortDateString() + ": " + video.Title,
                            CreationDate = timestamp
                        };
                        newVideo.Comments = GetVideoComments(newVideo.Id);
                        history.Add(newVideo);
                    }
                    cursor = videos.Pagination.Cursor;
                } while (cursor != null);
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
                            Timestamp = comment.CreatedAt.ToLocalTime(),
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
}