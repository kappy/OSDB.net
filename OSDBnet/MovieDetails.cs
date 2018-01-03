using System.Collections.Generic;

namespace OSDBnet
{
    public class MovieDetails
    {
        public string Rating { get; set; }
        public string Cover { get; set; }
        public string Id { get; set; }
        public string Votes { get; set; }
        public string Title { get; set; }
        public string[] Aka { get; set; }
        public string Year { get; set; }
        public IDictionary<string, string> Cast { get; set; }
        public IDictionary<string, string> Writers { get; set; }
        public string Trivia { get; set; }
        public string[] Genres { get; set; }
        public string[] Country { get; set; }
        public string[] Language { get; set; }
        public IDictionary<string, string> Directors { get; set; }
        public string Duration { get; set; }
        public string Tagline { get; set; }
    }
}