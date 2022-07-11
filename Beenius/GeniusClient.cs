﻿using MusicBeePlugin;
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
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private HttpClient client = new HttpClient();

        private string Token = "ZTejoT_ojOEasIkT9WrMBhBQOz6eYKK5QULCMECmOhvwqjRZ6WbpamFe3geHnvp3"; //anonymous Android app token
        private string ApiURL = "https://api.genius.com";
        private int AllowedDistance = 5; //a number of edits needed to get from one title to another
        private char[] Delimiters = { }; //delimiters to remove additional authors from the string
        private int MaxResults = 1; //maximum search results to analyze
        public GeniusClient()
        {
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

                Logger.Info("Configuration file was used: allowedDistance={allowedDistance}, delimiters={delimiters}, token={token}, maxResults={maxResults}", AllowedDistance, Delimiters, Token, MaxResults);
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
            Logger.Info("Attempting to search for {aritst} - {title}", artist, title);

            string result = search(artist, title);
            if (string.IsNullOrEmpty(result) && Delimiters.Length > 0)
            {
                foreach (char delimiter in Delimiters) artist = artist.Split(delimiter)[0].Trim();

                Logger.Info("Nothing found, attempting to search for {aritst} - {title}", artist, title);

                result = search(artist, title);
            }
            if (string.IsNullOrEmpty(result)) { Logger.Info("Nothing found at all"); }
            else { Logger.Info("Got a hit"); }
            return result;
        }

        private string search(string artist, string title)
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
            dynamic songPage = GeniusRequest(songApiPath);
            dynamic lyricsDom = songPage.response.song.lyrics.dom.children;

            string result = parseLyricsDom(lyricsDom);

            Logger.Info("Found lyrics");

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
