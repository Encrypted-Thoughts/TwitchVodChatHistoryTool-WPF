using Newtonsoft.Json;

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

    public class CommentVideoContainer
    {
        public Data data { get; set; }
        public Extensions extensions { get; set; }
    }

    public class Data
    {
        public CommentVideo video { get; set; }
    }

    public class CommentVideo
    {
        public string id { get; set; }
        public Creator creator { get; set; }
        public Comments comments { get; set; }
    }

    public class Creator
    {
        public string id { get; set; }
        public Channel channel { get; set; }
    }

    public class Channel
    {
        public string id { get; set; }
    }

    public class Comments
    {
        public Edge[] edges { get; set; }
        public Pageinfo pageInfo { get; set; }
    }

    public class Pageinfo
    {
        public bool hasNextPage { get; set; }
        public bool hasPreviousPage { get; set; }
    }

    public class Edge
    {
        public string cursor { get; set; }
        public Node node { get; set; }
    }

    public class Node
    {
        public string id { get; set; }
        public Commenter commenter { get; set; }
        public int contentOffsetSeconds { get; set; }
        public DateTime createdAt { get; set; }
        public Message message { get; set; }
    }

    public class Commenter
    {
        public string id { get; set; }
        public string login { get; set; }
        public string displayName { get; set; }
    }

    public class Message
    {
        public Fragment[] fragments { get; set; }
        public Userbadge[] userBadges { get; set; }
        public string userColor { get; set; }
    }

    public class Fragment
    {
        public Emote emote { get; set; }
        public string text { get; set; }
    }

    public class Emote
    {
        public string id { get; set; }
        public string emoteID { get; set; }
        public int from { get; set; }
        public string name { get; set; }
    }

    public class Userbadge
    {
        public string id { get; set; }
        public string setID { get; set; }
        public string version { get; set; }
    }

    public class Extensions
    {
        public int durationMilliseconds { get; set; }
        public string operationName { get; set; }
        public string requestID { get; set; }
    }

}
