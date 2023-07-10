using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GamerMatch.Controllers
{
    public class ApiController : Controller
    {
        private IConfiguration _config;
        private JsonDocument jDoc;

        public ApiController(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> ValidateSteamID(string steam)
        {
            bool valid = false;

            using (var httpClient = new HttpClient())
            {
                var apiKey = _config["ApiKey"];

                using (var response =
                    await httpClient.GetAsync($" http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={steam}"))
                {
                    var summary = await response.Content.ReadAsStringAsync();
                    jDoc = JsonDocument.Parse(summary);
                    var players = jDoc.RootElement.GetProperty("response").GetProperty("players");

                    for (int i = 0; i < players.GetArrayLength(); i++)
                    {
                        var player = players[i].GetProperty("steamid").GetString();

                        if (player == steam)
                        {
                            valid = true;
                            return valid;
                        }
                    }

                }
            }

            return valid;
        }

        // Calls API to return a Steam ID if the user owns a game from the search parameter
        public async Task<string> SearchGames(string steamID, string gameSearch)
        {
            using (var httpClient = new HttpClient())
            {
                var apiKey = _config["ApiKey"];

                using (var response =
                    await httpClient.GetAsync($"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={steamID}&include_appinfo=1"))
                {
                    var summary = await response.Content.ReadAsStringAsync();
                    jDoc = JsonDocument.Parse(summary);
                    var games = jDoc.RootElement.GetProperty("response").GetProperty("games");

                    for (int i = 0; i < games.GetArrayLength(); i++)
                    {
                        var game = games[i].GetProperty("name").GetString();

                        if (game == gameSearch)
                        {
                            return steamID;
                        }
                    }

                    return null;
                }
            }
        }

        // Calls API to return a list of recently played games to populate the search view
        public async Task<List<string>> MyGames(string steamID)
        {
            using (var httpClient = new HttpClient())
            {
                var apiKey = _config["ApiKey"];
                List<string> myGames = new List<string>();

                using (var response =
                    await httpClient.GetAsync($"http://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v0001/?key={apiKey}&steamid={steamID}&format=json"))
                {
                    var summary = await response.Content.ReadAsStringAsync();
                    jDoc = JsonDocument.Parse(summary);
                    var games = jDoc.RootElement.GetProperty("response").GetProperty("games");

                    for (int i = 0; i < games.GetArrayLength(); i++)
                    {
                        var game = games[i].GetProperty("name").GetString();
                        myGames.Add(game);
                    }

                    myGames.Sort();
                    return myGames;
                }
            }
        }

        public async Task<List<string>> GetSteamLibrary(string steamID)
        {
            List<string> library = new List<string>();

            using (var httpClient = new HttpClient())
            {
                var apiKey = _config["ApiKey"];

                using (var response =
                    await httpClient.GetAsync($"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={steamID}&include_appinfo=1"))
                {
                    var summary = await response.Content.ReadAsStringAsync();
                    jDoc = JsonDocument.Parse(summary);
                    var games = jDoc.RootElement.GetProperty("response").GetProperty("games");

                    for (int i = 0; i < games.GetArrayLength(); i++)
                    {
                        var game = games[i].GetProperty("name").GetString();

                        library.Add(game);
                    }

                    return library;
                }
            }
        }
    }
}
