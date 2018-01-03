using CookComputing.XmlRpc;

namespace OSDBnet.Backend
{ 
	[XmlRpcUrl("http://api.opensubtitles.org/xml-rpc")]
	public interface IOsdb : IXmlRpcProxy {

		[XmlRpcMethod("LogIn")]
		LoginResponse Login(string username, string password, string language, string useragent);

		[XmlRpcMethod("LogOut")]
		ResponseBase Logout(string token);

		[XmlRpcMethod("SearchSubtitles")]
		SearchSubtitlesResponse SearchSubtitles(string token, SearchSubtitlesRequest[] request);

		[XmlRpcMethod("CheckSubHash")]
		CheckSubHashResponse CheckSubHash(string token, string[] subhashes);

		[XmlRpcMethod("CheckMovieHash2")]
		CheckMovieHashResponse CheckMovieHash(string token, string[] moviehash);

		[XmlRpcMethod("GetSubLanguages")]
		GetSubLanguagesResponse GetSubLanguages(string language);

		[XmlRpcMethod("SearchMoviesOnIMDB")]
		SearchMoviesOnIMDBResponse SearchMoviesOnIMDB(string token, string query);

		[XmlRpcMethod("GetIMDBMovieDetails")]
		GetIMDBMovieDetailsResponse GetIMDBMovieDetails(string token, string imdbId);

		[XmlRpcMethod("NoOperation")]
		ResponseBase NoOperation(string token);

		[XmlRpcMethod("GetComments")]
		GetCommentsResponse GetComments(string token, string[] idsubtitle);

		[XmlRpcMethod("DetectLanguage")]
		DetectLanguageResponse DetectLanguage(string token, string[] text);

		[XmlRpcMethod("ReportWrongMovieHash")]
		ResponseBase ReportWrongMovieHash(string token, string IDSubMovieFile);
	}
}
