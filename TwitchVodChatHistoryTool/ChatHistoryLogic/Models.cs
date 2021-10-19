using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ChatHistory.Models
{
    public class Video
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
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
