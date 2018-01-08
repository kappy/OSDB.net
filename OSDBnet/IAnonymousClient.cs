using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSDBnet
{
    public interface IAnonymousClient : IDisposable
    {
        Task<IList<Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId, int? season, int? episode);
        Task<IEnumerable<Language>> GetSubLanguages();
        Task<IList<Subtitle>> SearchSubtitlesFromFile(string languages, string filename);
        Task<IList<Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId);
        Task<IList<Subtitle>> SearchSubtitlesFromQuery(string languages, string query, int? season = null, int? episode = null);
        Task<string> DownloadSubtitleToPath(string path, Subtitle subtitle, bool remote = true);
        Task<string> DownloadSubtitleToPath(string path, Subtitle subtitle, string newSubtitleName, bool remote = true);
        Task<long> CheckSubHash(string subHash);
        Task<IEnumerable<MovieInfo>> CheckMovieHash(string moviehash);
        Task<IEnumerable<Language>> GetSubLanguages(string language);
        Task<IEnumerable<Movie>> SearchMoviesOnImdb(string query);
        Task<MovieDetails> GetImdbMovieDetails(string imdbId);
        //Should this be exposed?
        Task NoOperation();
        Task<IEnumerable<UserComment>> GetComments(string idSubtitle);
        Task<string> DetectLanguge(string data);
        Task ReportWrongMovieHash(string idSubMovieFile);
    }
}