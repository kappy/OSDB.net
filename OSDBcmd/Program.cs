using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using OSDBnet;

namespace OSDBcmd {
	class Program {

		private const string EXECUTABLENAME = "OSDB";

		static void Main(string[] args) {
			bool lucky = false;
			bool showHelp = false;
			string movieName = null;
			List<string> languages = new List<string>();

			OptionSet opts = new OptionSet() { 
				{ "help", "Help Me!", v => showHelp = v != null },
				{ "m|movie=", "Movie file", v => movieName = v },
				{ "lucky", "Feeling lucky", v => lucky = v != null },
				{ "l|languages=", "Languages", v => languages.Add(v) }
			};

			List<string> extra;
			try {
				extra = opts.Parse(args);
			} catch (OptionException e) {
				ShowError(e.Message);
				Console.WriteLine("Try `{0} --help' for more information.", EXECUTABLENAME);
				return;
			}

			if (showHelp) {
				ShowHelp(opts);
				return;
			}

			if (string.IsNullOrEmpty(movieName)) {
				ShowError("You must specify a movie file name");
				return;
			}

			if (!Path.IsPathRooted(movieName)) {
				movieName = Path.Combine(Environment.CurrentDirectory, movieName);
			}
			if (!File.Exists(movieName)) {
				ShowError(string.Format("Could not find the movie file '{0}'", movieName));
				return;
			}
			try {
				DownloadSubtitle(movieName, lucky, languages);
				Console.WriteLine("Thanks for using this utility.");
				Console.WriteLine("All subtitles are on OpenSubtitles.org");
			} catch (Exception ex) {
				Console.WriteLine("ops.... something went wrong.");
				Console.WriteLine("{0} : {1}", ex.GetType().Name, ex.Message);
			}
		}

		static void DownloadSubtitle(string movieFileName, bool lucky, IList<string> languages) {
			var systemLanguage = GetSystemLanguage();
			if (!languages.Contains(systemLanguage)) 
		        	languages.Add(systemLanguage);
			using (var osdb = Osdb.Login(systemLanguage, "OS Test User Agent")) {
				var subtitles = osdb.SearchSubtitlesFromFile(languages.Aggregate( (a,b) => a + "," +b ),movieFileName);

				int subtitlesCount = subtitles.Count;
				if (subtitlesCount == 0) {
					Console.WriteLine("Sorry, no subtitles found for your movie");
					return;
				}
				var selectedSubtitle = subtitles.First();
				if (!lucky) {
					var canceled = !PromptForSubtitle(subtitles, ref selectedSubtitle);
					if (canceled) {
						return;
					}
				}

				string subtitleFile = osdb.DownloadSubtitleToPath(Path.GetDirectoryName(movieFileName), selectedSubtitle);
				Console.WriteLine("Subtitle downloaded to '{0}'", subtitleFile);
			}
		}

		private static bool PromptForSubtitle(IList<Subtitle> subtitles, ref Subtitle selectedSubtitle) {
			Console.WriteLine("I found multiple subtitles for your movie. Please chose one from the list:");
			DisplayChoices(subtitles, selectedSubtitle);
			string input;
			int choice = 0;
			bool validOption = false;
			do {
				Console.Write("Your choice? (avaible options: '1'-'{0}', 'c', 'l')  [{1}]:", subtitles.Count, subtitles.IndexOf(selectedSubtitle) + 1);
				input = Console.ReadLine();
				if (int.TryParse(input, out choice)) {
					if (choice > 0 || choice <= subtitles.Count) {
						validOption = true;
					}
				}
				
				if (!validOption) {
					if (string.Compare(input, "l", StringComparison.InvariantCultureIgnoreCase) == 0) {
						DisplayChoices(subtitles, selectedSubtitle);
					} else if (string.Compare(input, "c", StringComparison.InvariantCultureIgnoreCase) == 0) {
						return false;
					} else {
						Console.WriteLine("Invalid option. Please chose one of the index numbers from the list, 'c' to cancel or 'l' to show the list again.");
					}
				}
			} while (!validOption);

			selectedSubtitle = subtitles[choice - 1];

			return true;
		}

		private static void DisplayChoices(IList<Subtitle> subtitles, Subtitle defaultSubtitle) {
			int choice = 1;
			foreach (var subtitle in subtitles) {
				Console.WriteLine("{0}[{1}] {2}", subtitle.Equals(defaultSubtitle)? "*": " ", choice++, subtitle.MovieName);
				Console.WriteLine("\tFile:{0}", subtitle.SubtitleFileName);
				Console.WriteLine("\tLanguage:{0}", subtitle.LanguageName);
				Console.WriteLine();
			}
		}

		static void ShowError(string message) {
			Console.WriteLine(message);
		}

		static void ShowHelp(OptionSet p) {
			Console.WriteLine("Usage: {0} -m filename [OPTIONS]+", EXECUTABLENAME);
			Console.WriteLine("A subtitle dowloader utility");
			Console.WriteLine();
			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
		}

		static string GetSystemLanguage() {
			var currentCulture = System.Globalization.CultureInfo.CurrentUICulture;
			return currentCulture.TwoLetterISOLanguageName.ToLower();
		}
	}
}
