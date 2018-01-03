using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CookComputing.XmlRpc;
using OSDBnet.Backend;

namespace OSDBnet
{
    public class AnonymousClient : IAnonymousClient
    {
        private bool Disposed { get; set; }
        private IOsdb Proxy { get; }
        private string Token { get; set; }

        internal AnonymousClient(IOsdb proxy)
        {
            Proxy = proxy;
        }

        internal Task Login(string username, string password, string language, string userAgent)
        {
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.Login(username, password, language, userAgent);
                    VerifyResponseCode(response);
                    Token = response.token;
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<IList<Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId, int? season, int? episode)
        {
            if (string.IsNullOrEmpty(imdbId))
            {
                throw new ArgumentNullException(nameof(imdbId));
            }

            var request = new SearchSubtitlesRequest
            {
                sublanguageid = languages,
                imdbid = imdbId,
                episode = episode,
                season = season
            };

            return SearchSubtitlesInternal(request);
        }

        public Task<IList<Subtitle>> SearchSubtitlesFromFile(string languages, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            var file = new FileInfo(filename);
            if (!file.Exists)
            {
                throw new ArgumentException("File doesn't exist", nameof(filename));
            }
            var request = new SearchSubtitlesRequest
            {
                sublanguageid = languages,
                moviehash = HashHelper.ToHexadecimal(HashHelper.ComputeMovieHash(filename)),
                moviebytesize = file.Length.ToString(),
                imdbid = string.Empty,
                query = string.Empty
            };


            return SearchSubtitlesInternal(request);
        }

        public Task<IList<Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId)
        {
            if (string.IsNullOrEmpty(imdbId))
            {
                throw new ArgumentNullException(nameof(imdbId));
            }
            var request = new SearchSubtitlesRequest
            {
                sublanguageid = languages,
                imdbid = imdbId
            };

            return SearchSubtitlesInternal(request);
        }

        public Task<IList<Subtitle>> SearchSubtitlesFromQuery(string languages, string query, int? season = null,
            int? episode = null)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query));
            }
            var request = new SearchSubtitlesRequest
            {
                sublanguageid = languages,
                query = query,
                season = season,
                episode = episode
            };

            return SearchSubtitlesInternal(request);
        }

        public Task<long> CheckSubHash(string subHash)
        {
            var tcs = new TaskCompletionSource<long>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.CheckSubHash(Token, new[] {subHash});
                    VerifyResponseCode(response);

                    long idSubtitleFile = 0;
                    var hashInfo = response.data as XmlRpcStruct;
                    if (hashInfo != null && hashInfo.ContainsKey(subHash))
                    {
                        idSubtitleFile = Convert.ToInt64(hashInfo[subHash]);
                    }

                    tcs.TrySetResult(idSubtitleFile);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<IEnumerable<MovieInfo>> CheckMovieHash(string moviehash)
        {
            var tcs = new TaskCompletionSource<IEnumerable<MovieInfo>>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.CheckMovieHash(Token, new[] {moviehash});
                    VerifyResponseCode(response);

                    var movieInfoList = new List<MovieInfo>();

                    var hashInfo = response.data as XmlRpcStruct;
                    if (hashInfo != null && hashInfo.ContainsKey(moviehash))
                    {
                        var movieInfoArray = hashInfo[moviehash] as object[];
                        if (movieInfoArray != null)
                        {
                            foreach (XmlRpcStruct movieInfoStruct in movieInfoArray)
                            {
                                var movieInfo = SimpleObjectMapper.MapToObject<CheckMovieHashInfo>(movieInfoStruct);
                                movieInfoList.Add(BuildMovieInfoObject(movieInfo));
                            }
                        }
                    }

                    tcs.TrySetResult(movieInfoList);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<IEnumerable<Language>> GetSubLanguages(string language)
        {
            var tcs = new TaskCompletionSource<IEnumerable<Language>>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.GetSubLanguages(language);
                    VerifyResponseCode(response);

                    IList<Language> languages = new List<Language>();
                    foreach (var languageInfo in response.data)
                    {
                        languages.Add(BuildLanguageObject(languageInfo));
                    }

                    tcs.TrySetResult(languages);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<IEnumerable<Movie>> SearchMoviesOnImdb(string query)
        {
            var tcs = new TaskCompletionSource<IEnumerable<Movie>>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.SearchMoviesOnIMDB(Token, query);
                    VerifyResponseCode(response);

                    IList<Movie> movies = new List<Movie>();

                    if (response.data.Length == 1 && string.IsNullOrEmpty(response.data.First().id))
                    {
                        // no match found
                        tcs.TrySetResult(movies);
                        return;
                    }

                    foreach (var movieInfo in response.data)
                    {
                        movies.Add(BuildMovieObject(movieInfo));
                    }

                    tcs.TrySetResult(movies);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<MovieDetails> GetImdbMovieDetails(string imdbId)
        {
            var tcs = new TaskCompletionSource<MovieDetails>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.GetIMDBMovieDetails(Token, imdbId);
                    VerifyResponseCode(response);

                    var movieDetails = BuildMovieDetailsObject(response.data);
                    tcs.TrySetResult(movieDetails);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task NoOperation()
        {
            return Task.Run(() =>
            {
                var response = Proxy.NoOperation(Token);
                VerifyResponseCode(response);
            });
        }

        public Task<IEnumerable<UserComment>> GetComments(string idsubtitle)
        {
            var tcs = new TaskCompletionSource<IEnumerable<UserComment>>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.GetComments(Token, new[] {idsubtitle});
                    VerifyResponseCode(response);

                    var comments = new List<UserComment>();
                    var commentsStruct = response.data as XmlRpcStruct;
                    if (commentsStruct == null)
                    {
                        tcs.TrySetResult(comments);
                        return;
                    }

                    if (commentsStruct.ContainsKey("_" + idsubtitle))
                    {
                        var commentsList = commentsStruct["_" + idsubtitle] as object[];
                        if (commentsList != null)
                        {
                            foreach (XmlRpcStruct commentStruct in commentsList)
                            {
                                var comment = SimpleObjectMapper.MapToObject<CommentsData>(commentStruct);
                                comments.Add(BuildUserCommentObject(comment));
                            }
                        }
                    }

                    tcs.TrySetResult(comments);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<string> DetectLanguge(string data)
        {
            var tcs = new TaskCompletionSource<string>();
            Task.Run(() =>
            {
                try
                {
                    var bytes = GzipString(data);
                    var text = Convert.ToBase64String(bytes);

                    var response = Proxy.DetectLanguage(Token, new[] {text});
                    VerifyResponseCode(response);

                    var languagesStruct = response.data as XmlRpcStruct;
                    if (languagesStruct == null)
                    {
                        tcs.TrySetResult(null);
                        return;
                    }

                    foreach (string key in languagesStruct.Keys)
                    {
                        tcs.TrySetResult(languagesStruct[key].ToString());
                        return;
                    }

                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task ReportWrongMovieHash(string idSubMovieFile)
        {
            return Task.Run(() =>
            {
                var response = Proxy.ReportWrongMovieHash(Token, idSubMovieFile);
                VerifyResponseCode(response);
            });
        }

        public Task<string> DownloadSubtitleToPath(string path, Subtitle subtitle, bool remote = true)
        {
            return DownloadSubtitleToPath(path, subtitle, null, remote);
        }

        public async Task<string> DownloadSubtitleToPath(string path, Subtitle subtitle, string newSubtitleName,
            bool remote = true)
        {
            var destinationfile = Path.Combine(path,
                (string.IsNullOrEmpty(newSubtitleName)) ? subtitle.SubtitleFileName : newSubtitleName);
            if (remote)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (!Directory.Exists(path))
                {
                    throw new ArgumentException("path should point to a valid location");
                }

                if (File.Exists(destinationfile))
                {
                    //if file has been downloaded before - there is no need to download it again
                    return destinationfile;
                }

                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(subtitle.SubTitleDownloadLink))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var content = response.Content)
                        {
                            var decompressed = await UnZipSubtitleFileToFile(await content.ReadAsByteArrayAsync());
                            await DecodeAndWriteFile(subtitle.ISO639.ToLower(), destinationfile, decompressed);
                        }
                    }
                }
            }
            else
            {
                await DecodeAndWriteFile(string.Empty, destinationfile,
                    File.ReadAllBytes(subtitle.SubTitleDownloadLink.AbsolutePath));
            }

            return destinationfile;
        }

        public Task<IEnumerable<Language>> GetSubLanguages()
        {
            //get system language
            return GetSubLanguages("en");
        }

        private static byte[] GzipString(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }

        private static Subtitle BuildSubtitleObject(SearchSubtitlesInfo info)
        {
            var sub = new Subtitle
            {
                SubtitleId = info.IDSubtitle,
                SubtitleHash = info.SubHash,
                SubtitleFileName = info.SubFileName,
                SubTitleDownloadLink = new Uri(info.SubDownloadLink),
                SubtitlePageLink = new Uri(info.SubtitlesLink),
                LanguageId = info.SubLanguageID,
                LanguageName = info.LanguageName,
                Rating = info.SubRating,
                Bad = info.SubBad,

                ImdbId = info.IDMovieImdb,
                MovieId = info.IDMovie,
                MovieName = info.MovieName,
                OriginalMovieName = info.MovieNameEng,
                MovieYear = int.Parse(info.MovieYear)
            };
            return sub;
        }

        private static MovieInfo BuildMovieInfoObject(CheckMovieHashInfo info)
        {
            var movieInfo = new MovieInfo
            {
                MovieHash = info.MovieHash,
                MovieImdbID = info.MovieImdbID,
                MovieYear = info.MovieYear,
                MovieName = info.MovieName,
                SeenCount = info.SeenCount
            };
            return movieInfo;
        }

        private static Language BuildLanguageObject(GetSubLanguagesInfo info)
        {
            var language = new Language
            {
                LanguageName = info.LanguageName,
                SubLanguageID = info.SubLanguageID,
                ISO639 = info.ISO639
            };
            return language;
        }

        private static Movie BuildMovieObject(MoviesOnIMDBInfo info)
        {
            var movie = new Movie
            {
                Id = Convert.ToInt64(info.id),
                Title = info.title
            };
            return movie;
        }

        private static MovieDetails BuildMovieDetailsObject(IMDBMovieDetails info)
        {
            var movie = new MovieDetails
            {
                Aka = info.aka,
                Cast = SimpleObjectMapper.MapToDictionary(info.cast as XmlRpcStruct),
                Cover = info.cover,
                Id = info.id,
                Rating = info.rating,
                Title = info.title,
                Votes = info.votes,
                Year = info.year,
                Country = info.country,
                Directors = SimpleObjectMapper.MapToDictionary(info.directors as XmlRpcStruct),
                Duration = info.duration,
                Genres = info.genres,
                Language = info.language,
                Tagline = info.tagline,
                Trivia = info.trivia,
                Writers = SimpleObjectMapper.MapToDictionary(info.writers as XmlRpcStruct)
            };
            return movie;
        }

        private static UserComment BuildUserCommentObject(CommentsData info)
        {
            var comment = new UserComment
            {
                Comment = info.Comment,
                Created = info.Created,
                IDSubtitle = info.IDSubtitle,
                UserID = info.UserID,
                UserNickName = info.UserNickName
            };
            return comment;
        }

        private static void VerifyResponseCode(ResponseBase response)
        {
            if (null == response)
            {
                throw new ArgumentNullException(nameof(response));
            }
            if (string.IsNullOrEmpty(response.status))
            {
                //aperantly there are some methods that dont define 'status'
                return;
            }

            int responseCode = int.Parse(response.status.Substring(0, 3));
            if (responseCode >= 400)
            {
                throw new OsdbException($"Unexpected error response {response.status}");
            }
        }

        private Task<IList<Subtitle>> SearchSubtitlesInternal(SearchSubtitlesRequest request)
        {
            var tcs = new TaskCompletionSource<IList<Subtitle>>();
            Task.Run(() =>
            {
                try
                {
                    var response = Proxy.SearchSubtitles(Token, new[] {request});
                    VerifyResponseCode(response);

                    var subtitles = new List<Subtitle>();
                    var subtitlesInfo = response.data as object[];
                    if (subtitlesInfo != null)
                    {
                        foreach (var infoObject in subtitlesInfo)
                        {
                            var subInfo =
                                SimpleObjectMapper.MapToObject<SearchSubtitlesInfo>((XmlRpcStruct) infoObject);
                            subtitles.Add(BuildSubtitleObject(subInfo));
                        }
                    }
                    tcs.TrySetResult(subtitles);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }


        private async Task DecodeAndWriteFile(string strLang, string destinationfile, byte[] decompressed)
        {
            using (var subFile = new StreamWriter(destinationfile, false, Encoding.UTF8))
            {
                var enc = Encoding.UTF8;
                if (strLang == "he") enc = Encoding.GetEncoding("windows-1255");
                if (strLang == "el") enc = Encoding.GetEncoding("windows-1253");
                if (strLang == "ar") enc = Encoding.GetEncoding("windows-1256");
                var str = Encoding.Convert(enc, Encoding.UTF8, decompressed);
                await subFile.WriteAsync(WebUtility.HtmlDecode(Encoding.UTF8.GetString(str)));
            }
        }

        private async Task<byte[]> UnZipSubtitleFileToFile(byte[] gzipStream)
        {
            using (var compressedMs = new MemoryStream(gzipStream))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs,
                        CompressionMode.Decompress)))
                    {
                        await gzs.CopyToAsync(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing && !string.IsNullOrEmpty(Token))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            Proxy.Logout(Token);
                        }
                        catch
                        {
                            //soak it. We don't want exception on disposing. It's better to let the session timeout.
                        }
                        Token = null;
                    });
                }
                Disposed = true;
            }
        }

        ~AnonymousClient()
        {
            Dispose(false);
        }
    }
}