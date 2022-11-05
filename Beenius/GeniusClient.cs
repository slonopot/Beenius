using MusicBeePlugin;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Topten.JsonKit;

namespace Beenius
{
    public class GeniusClient
    {
        private static Logger Logger = LogManager.GetLogger("Beenius");

        private HttpClient client = new HttpClient();

        private string LyricsProviderName;

        private string Token = "ZTejoT_ojOEasIkT9WrMBhBQOz6eYKK5QULCMECmOhvwqjRZ6WbpamFe3geHnvp3"; //anonymous Android app token
        private string ApiURL = "https://api.genius.com";
        private int AllowedDistance = 5; //a number of edits needed to get from one title to another
        private char[] Delimiters = { }; //delimiters to remove additional authors from the string
        private int MaxResults = 1; //maximum search results to analyze
        private bool AddLyricsSource = false;
        private bool TrimTitle = false;
        public GeniusClient(string lyricsProviderName = null)
        {
            LyricsProviderName = lyricsProviderName;

            client.DefaultRequestHeaders.Remove("User-Agent");
            client.DefaultRequestHeaders.Add("User-Agent", "okhttp/4.9.1");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);

            if (File.Exists(Plugin.configFile))
            {
                string data = File.ReadAllText(Plugin.configFile);
                dynamic config = Json.Parse<object>(data);
                if (Util.PropertyExists(config, "allowedDistance"))
                    AllowedDistance = (int)config.allowedDistance;
                if (Util.PropertyExists(config, "delimiters"))
                    Delimiters = ((List<object>)config.delimiters).Select(x => char.Parse(x.ToString())).ToArray();
                if (Util.PropertyExists(config, "token"))
                    Token = config.token;
                if (Util.PropertyExists(config, "maxResults"))
                    MaxResults = (int)config.maxResults;
                if (Util.PropertyExists(config, "addLyricsSource"))
                    AddLyricsSource = (bool)config.addLyricsSource;
                if (Util.PropertyExists(config, "trimTitle"))
                    TrimTitle = (bool)config.trimTitle;

                Logger.Info("Configuration file was used: allowedDistance={allowedDistance}, delimiters={delimiters}, token={token}, maxResults={maxResults}, addLyricsSource={addLyricsSource}, trimTitle={trimTitle}", AllowedDistance, Delimiters, Token, MaxResults, AddLyricsSource, TrimTitle);
            }
            else { Logger.Info("No configuration file was provided, defaults were used"); }
        }

        private dynamic GeniusRequest(string path, NameValueCollection parameters = null)
        {
            string url = this.ApiURL + path;
            if (parameters == null)
                parameters = new NameValueCollection();

            parameters.Add("from_background", "0");

            url += "?" + Util.ToQueryString(parameters);
            HttpResponseMessage response = null;
            try
            {
                var task = Task.Run(() => client.GetAsync(url));
                task.Wait();
                response = task.Result;
            }
            catch (Exception)
            {
                throw;
            }
            dynamic result = null;
            try
            {
                string content = string.Empty;
                var task = Task.Run(() => response.Content.ReadAsStringAsync());
                task.Wait();
                content = task.Result;
                result = Json.Parse<object>(content);
            }
            catch { throw; }
            return result;
        }

        public string getLyrics(string artist, string title)
        {
            artist = artist.Trim();
            title = title.Trim();

            if (TrimTitle) { title = Util.Trim(title); }

            Logger.Info("Attempting to search for {aritst} - {title}", artist, title);

            string result = search(artist, title);
            if (string.IsNullOrEmpty(result) && Delimiters.Length > 0)
            {
                var editedArtist = artist;

                foreach (char delimiter in Delimiters) editedArtist = editedArtist.Split(delimiter)[0].Trim();

                if (editedArtist != artist)
                {
                    Logger.Info("Nothing found, attempting to search for {aritst} - {title}", editedArtist, title);

                    result = search(editedArtist, title);
                }
            }
            if (string.IsNullOrEmpty(result)) { Logger.Info("Nothing found at all"); }
            else { Logger.Info("Got a hit"); }
            return result;
        }

        private string search(string artist, string title, bool parseDom = true)
        {
            Logger.Debug("artist={artist}, title={title}", artist, title);

            var req = new NameValueCollection();
            req.Add("q", artist + " " + title);
            dynamic searchResults = GeniusRequest("/search", req);
            var matches = searchResults.response.hits;
            if (matches.Count == 0) { return null; }

            dynamic chosenMatch = null;

            int analyzedMatches = 0;

            foreach (var match in matches)
            {
                if (analyzedMatches > MaxResults - 1) break;

                if (match.type != "song") continue;

                if (Util.ValidateResult(artist, title, match.result.primary_artist.name, match.result.title, AllowedDistance))
                {
                    chosenMatch = match;
                    break;
                }

                Logger.Info("Let's check for aliases");

                dynamic matchArtistResult = GeniusRequest(match.result.primary_artist.api_path);
                dynamic matchArtist = matchArtistResult.response.artist;
                if (matchArtist.alternate_names.Count == 0) { Logger.Info("No aliases"); }

                foreach (var alias in matchArtist.alternate_names)
                {
                    if (Util.ValidateResult(artist, title, alias, match.result.title, AllowedDistance))
                    {
                        chosenMatch = match;
                        break;
                    }
                }

                if (chosenMatch != null) break;

                analyzedMatches++;
            }

            if (chosenMatch == null)
            {
                Logger.Info("No results for this search");

                return null;
            }

            string songApiPath = chosenMatch.result.api_path;

            string result = string.Empty;
            
            if (parseDom)
            {
                dynamic songPage = GeniusRequest(songApiPath);
                
                dynamic lyricsDom = songPage.response.song.lyrics.dom.children;

                result = parseLyricsDom(lyricsDom);
            }
            else //Won't use plain text since Genius returns it with a space before every line. Also I've already done DOM parsing so why bother.
            {
                req = new NameValueCollection();
                req.Add("text_format", "plain");

                dynamic songPage = GeniusRequest(songApiPath, req);

                result = songPage.response.song.lyrics.plain;
            }

            Logger.Info("Found lyrics");

            if (AddLyricsSource)
                result = $"Source: {LyricsProviderName}\n\n" + result;

            return result;
        }

        private string parseLyricsDom(dynamic lyricsDom)
        {
            string result = string.Empty;

            foreach (dynamic elem in lyricsDom)
            {
                if (elem is System.Dynamic.ExpandoObject)
                {
                    if (Util.PropertyExists(elem, "tag") && elem.tag == "br") result += '\n';
                    if (!Util.PropertyExists(elem, "children")) continue;
                    result += parseLyricsDom(elem.children);
                }
                else if (elem is string)
                {
                    string entry = elem.ToString();
                    result += entry;
                }
                else
                {

                }
            }

            return result;
        }
    }
}
