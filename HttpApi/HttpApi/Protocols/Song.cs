using System;

namespace HttpApi.Protocols
{
    internal class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string[] Tags { get; set; }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, new[]
                {
                    string.Format("Artist: {0}", Artist),
                    string.Format("Title:  {0}", Title),
                    string.Format("Tags:   {0}", string.Join(",", Tags ?? new string[0]))
                });
        }
    }
}
