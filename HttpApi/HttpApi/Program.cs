using System;
using System.Linq;
using System.Text;
using HttpApi.Helpers;

namespace HttpApi
{
    class Program
    {
        private const int TopSize = 10;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var token = VkHelper.GetToken();
            var songs = VkHelper.GetSongs(token, ExtractId(args)).Result.ToArray();
            foreach (var song in songs)
            {
                song.Tags = LastFmHelper.GetTags(song).Result;
                Console.WriteLine(song);
                Console.WriteLine();
            }
            Console.WriteLine("Top {0} tags:", TopSize);
            var tags = songs
                .SelectMany(song => song.Tags)
                .Distinct()
                .Select(tag => new
                {
                    Name = tag, 
                    Count = songs.Count(song => song.Tags.Contains(tag))
                })
                .OrderByDescending(t => t.Count)
                .ToArray();
            foreach (var tag in tags.Take(TopSize))
            {
                Console.WriteLine("Tag: {0, -20} Count: {1}", tag.Name, tag.Count);
            }
        }

        static int? ExtractId(string[] args)
        {
            int id;
            return args.Length > 0 && int.TryParse(args[0], out id) ? id : (int?) null;
        }
    }
}
