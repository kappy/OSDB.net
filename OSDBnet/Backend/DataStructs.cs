using System;
using CookComputing.XmlRpc;

namespace OSDBnet.Backend
{
    public class ResponseBase
    {
        public string status;
        public double seconds;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class LoginResponse : ResponseBase
    {
        public string token;
    }

    public class SearchSubtitlesRequest
    {
        public string sublanguageid = string.Empty;
        public string moviehash = string.Empty;
        public string moviebytesize = string.Empty;
        public string imdbid = string.Empty;
        public string query = string.Empty;
        [XmlRpcMissingMapping(MappingAction.Ignore)] public int? season = null;
        [XmlRpcMissingMapping(MappingAction.Ignore)] public int? episode = null;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class SearchSubtitlesResponse : ResponseBase
    {
        public Object data;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class SearchSubtitlesInfo
    {
        public string SubFileName;
        public string SubHash;
        public string IDSubtitle;
        public string SubLanguageID;
        public string SubBad;
        public string SubRating;
        public string IDMovie;
        public string IDMovieImdb;
        public string MovieName;
        public string MovieNameEng;
        public string MovieYear;
        public string LanguageName;
        public string SubDownloadLink;
        public String ISO639;
        public string SubtitlesLink;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class CheckSubHashResponse : ResponseBase
    {
        public Object data;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class CheckMovieHashResponse : ResponseBase
    {
        public Object data;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class CheckMovieHashInfo
    {
        public string MovieHash;
        public string MovieImdbID;
        public string SeenCount;
        public string MovieYear;
        public string MovieName;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class GetSubLanguagesResponse : ResponseBase
    {
        public GetSubLanguagesInfo[] data;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class GetSubLanguagesInfo
    {
        public string SubLanguageID;
        public string LanguageName;
        public string ISO639;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class SearchMoviesOnIMDBResponse : ResponseBase
    {
        public MoviesOnIMDBInfo[] data;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class MoviesOnIMDBInfo
    {
        public string id;
        public string title;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class GetIMDBMovieDetailsResponse : ResponseBase
    {
        public IMDBMovieDetails data;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class IMDBMovieDetails
    {
        public object cast;
        public object writers;
        public string trivia;
        public string[] genres;
        public string[] country;
        public string[] language;
        public object directors;
        public string duration;
        public string tagline;
        public string rating;
        public string cover;
        public string id;
        public string votes;
        public string title;
        public string[] aka;
        public string year;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class GetCommentsResponse : ResponseBase
    {
        public object data;
    }

    public class CommentsData
    {
        public string IDSubtitle;
        public string UserID;
        public string UserNickName;
        public string Comment;
        public string Created;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class DetectLanguageResponse : ResponseBase
    {
        public object data;
    }
}