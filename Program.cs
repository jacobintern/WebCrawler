using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using System.Linq;
using System.Text;
using CsvHelper;
using System.IO;

namespace app
{
    public class players
    {
        public string name { get; set; }
        public string playerUrl { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            string[] letters = {
                "a","b","c","d","e","f","g","h","i","j",
                "k","l","m","n","o","p","q","r","s","t",
                "u","v","w","x","y","z"};
            var domain = "https://basketball-reference.com";
            foreach (var letter in letters)
            {
                await GetPlayerInfoByLetter(domain, letter);
            }
        }

        public static async Task GetPlayerInfoByLetter(string domain, string letter)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            using (var sw = new StreamWriter(string.Format("{0}/{1}.csv", Directory.GetCurrentDirectory(), letter)))
            using (var csv = new CsvWriter(sw, System.Globalization.CultureInfo.CurrentCulture))
            {
                csv.WriteField("Player");
                csv.WriteField("G");
                csv.WriteField("PTS");
                csv.WriteField("TRB");
                csv.WriteField("AST");
                csv.WriteField("FG(%)");
                csv.WriteField("FG3(%)");
                csv.WriteField("FT(%)");
                csv.WriteField("eFG(%)");
                csv.WriteField("PER");
                csv.WriteField("WS");
                csv.NextRecord();
                foreach (var player in GetPlayerUrlByLetter(domain, letter).Result)
                {
                    string url = string.Format("{0}{1}/", domain, player.playerUrl);
                    using (var document = await context.OpenAsync(url))
                    {
                        if (document.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var statsPullout = document.QuerySelectorAll("div").Where(x => x.ClassName == "stats_pullout").FirstOrDefault();
                            var p1 = statsPullout.QuerySelectorAll("div").Where(x => x.ClassName == "p1").FirstOrDefault().QuerySelectorAll("p").Where(x => x.TextContent != string.Empty);
                            var p2 = statsPullout.QuerySelectorAll("div").Where(x => x.ClassName == "p2").FirstOrDefault().QuerySelectorAll("p").Where(x => x.TextContent != string.Empty);
                            var p3 = statsPullout.QuerySelectorAll("div").Where(x => x.ClassName == "p3").FirstOrDefault().QuerySelectorAll("p").Where(x => x.TextContent != string.Empty);
                            csv.WriteField(player.name);
                            foreach (var ele in p1) { csv.WriteField(ele.TextContent); }
                            foreach (var ele in p2) { csv.WriteField(ele.TextContent); }
                            foreach (var ele in p3) { csv.WriteField(ele.TextContent); }
                            csv.NextRecord();
                        }
                    }
                }
            }
        }

        public static async Task<List<players>> GetPlayerUrlByLetter(string domain, string letter)
        {
            var result = new List<players>();
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            string url = string.Format("{0}/players/{1}/", domain, letter);

            using (var document = await context.OpenAsync(url))
            {
                if (document.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // get table
                    var players = document.QuerySelector("table").QuerySelector("tbody").QuerySelectorAll("tr");

                    foreach (var player in players)
                    {
                        // get player url
                        result.Add(
                            new players()
                            {
                                name = player.QuerySelector("th").QuerySelector("a").TextContent.ToString(),
                                playerUrl = player.QuerySelector("th").QuerySelector("a").GetAttribute("href").ToString()
                            });
                    }
                }
            }
            return result;
        }
    }
}