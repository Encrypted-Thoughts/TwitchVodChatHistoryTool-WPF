using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using TwitchLib.Api;
using ChatHistory.Models;
using System.Net.Http.Json;
using System.Text;
using TwitchLib.Api.Core.Enums;
using System.Net.Http.Headers;
using System.Threading;

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
                            Title = timestamp.ToShortDateString() + ": " + video.Title.Trim(),
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
            var baseUrl = $"https://gql.twitch.tv/gql";
            var offset = 0;

            int previous;
            var hasNextPage = true;
            do
            {
                previous = offset;
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(
                    "{\"operationName\":\"VideoCommentsByOffsetOrCursor\"," +
                    "\"variables\":{\"videoID\":\"" + id + "\",\"contentOffsetSeconds\":" + offset + "}," +
                    "\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"b70a3591ff0f4e0313d126c6a1502d79a1c02baebb288227c582044aa76adf6a\"}}}",
                    Encoding.UTF8, "application/json")
                };
                var response = TwitchGqlRequest(baseUrl, request);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<CommentVideoContainer>().Result;
                    if (result.data.video.comments == null) break;
                    foreach (var comment in result.data.video.comments.edges)
                    {
                        var simplified = new SimplifiedComment
                        {
                            Timestamp = comment.node.createdAt,
                            Username = comment.node.commenter.displayName,
                            Message = ""
                        };

                        foreach (var fragment in comment.node.message.fragments)
                            simplified.Message += fragment.emote == null ? fragment.text : fragment.emote.emoteID;

                        comments.Add(simplified);
                        offset = comment.node.contentOffsetSeconds;
                    }
                    hasNextPage = result.data.video.comments.pageInfo.hasNextPage;
                }
                else break;
            }
            while (offset != previous && hasNextPage);
            return comments;
        }

        private HttpResponseMessage TwitchGqlRequest(string url, HttpRequestMessage request)
        {
            var client = new HttpClient { BaseAddress = new Uri(url)};
            client.DefaultRequestHeaders.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            return client.SendAsync(request).Result;
        }
    }
}