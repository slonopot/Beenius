using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Topten.JsonKit;

namespace Beenius
{
    public class GeniusClient
    {
        private HttpClient client = new HttpClient();

        private string GeniusAnonymousAndroidToken = "ZTejoT_ojOEasIkT9WrMBhBQOz6eYKK5QULCMECmOhvwqjRZ6WbpamFe3geHnvp3";
        private string ApiURL = "https://api.genius.com";
        private int AllowedDistance = 5; //a number of edits needed to get from one title to another
        public GeniusClient()
        {
            client.DefaultRequestHeaders.Remove("User-Agent");
            client.DefaultRequestHeaders.Add("User-Agent", "okhttp/4.9.1");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GeniusAnonymousAndroidToken);
        }

        private dynamic GeniusRequest(string path, NameValueCollection parameters = null)
        {
            string url = this.ApiURL + path;
            if (parameters == null)
                parameters = new NameValueCollection();
            
            parameters.Add("from_background", "0");
            
            url += "?" + Util.ToQueryString(parameters);
            HttpResponseMessage response =  null;
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

        public string getLyrics(string artist, string title, string album)
        {
            var req = new NameValueCollection();
            req.Add("q", artist + " " + title);
            dynamic searchResults = GeniusRequest("/search", req);
            var matches = searchResults.response.hits;
            if (matches.Count == 0) { return null; }
            if (matches[0].type != "song") { return null; }
            string resultArtist = matches[0].result.artist_names;
            string resultTitle = matches[0].result.title;

            var requestedTitle = $"{artist} {title}".ToLower();
            var foundTitle = $"{resultArtist} {resultTitle}".ToLower();

            if (Util.ComputeDistance(requestedTitle, foundTitle) > AllowedDistance) { return null; }

            string songApiPath = matches[0].result.api_path;
            dynamic songPage = GeniusRequest(songApiPath);
            dynamic lyricsDom = songPage.response.song.lyrics.dom.children;
            
            string result = parseLyricsDom(lyricsDom);

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
                else if (elem is string){ 
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
