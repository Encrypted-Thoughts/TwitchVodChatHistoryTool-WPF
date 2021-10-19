using CommandLine;
using ChatHistory;
using System;
using System.Linq;

namespace TwitchVodChatHistoryConsole
{
    public class Program
    {
        public class Options
        {
            [Option('c', "channels", Required = true, SetName = "channel", HelpText = "Channel(s) for which to get chat history. Separate users by commas. Ex: \"encryptedthoughts,ninja,someothertwitchuser\"")]
            public string Channels { get; set; }

            [Option('v', "videos", Required = true, SetName = "vod", HelpText = "Videos(s) for which to get chat history. Separate videos by commas. Ex: \"1234567890,1234567891,1234567892\"")]
            public string Videos { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                if (o.Channels != null)
                {
                    Console.WriteLine($"Channels: {o.Channels}");
                    var chatHistoryHelper = new ChatHistoryLogic("vcv0nh16h9bwbl4cv0r3fhecna7c99");
                    var videos = chatHistoryHelper.GetChatHistoryByVideo(o.Channels.Split(',', StringSplitOptions.TrimEntries).ToList());

                    foreach(var video in videos)
                    {
                        Console.WriteLine($"{video.Title}");
                        foreach(var comment in video.Comments)
                        {
                            Console.WriteLine($"{comment.Timestamp} {comment.Username}: {comment.Message}");
                        }
                        Console.WriteLine("");
                    }
                }
                else
                {
                    Console.WriteLine($"Videos: {o.Videos}");
                }
            });
        }
    }
}