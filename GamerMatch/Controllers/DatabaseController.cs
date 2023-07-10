using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamerMatch.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GamerMatch.Controllers
{
    public class DatabaseController : Controller
    {
        private IConfiguration _config;
        private ApiController apiController;
        private HomeController homeController;

        public DatabaseController(IConfiguration config)
        {
            _config = config;
            apiController = new ApiController(config);
        }

        public async Task<List<AspNetUsers>> SearchSplit(string steamTitle, string boardTitle)
        {
            List<AspNetUsers> matchList = new List<AspNetUsers>();

            if (steamTitle == "null" && boardTitle != "null")
            {
                matchList = SearchMatchBoardGames(boardTitle);
            }
            else if (steamTitle != "null" && boardTitle == "null")
            {
                matchList = await SearchMatchSteam(steamTitle);
            }

            return matchList;
        }

        // Calls the API controller to compare the search parameter to another user's Steam library
        public async Task<List<AspNetUsers>> SearchMatchSteam(string gameSearch)
        {
            GamerMatchContext db = new GamerMatchContext();
            List<AspNetUsers> matchList = new List<AspNetUsers>();
            ViewBag.Title = gameSearch;

            foreach (AspNetUsers user in db.AspNetUsers)
            {
                if (user.SteamInfo != null)
                {
                    try
                    {
                        if (await apiController.SearchGames(user.SteamInfo, gameSearch) != null)
                        {
                            matchList.Add(user);
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                }
            }
            return matchList;
        }

        // Returns a list of users who match the board game search parameter
        public List<AspNetUsers> SearchMatchBoardGames(string gameTitle)
        {
            GamerMatchContext db = new GamerMatchContext();
            List<AspNetUsers> matchList = new List<AspNetUsers>(); 
            ViewBag.Title = gameTitle;
            List<string> gameList = new List<string>();
            string userGames = null;

            foreach (AspNetUsers user in db.AspNetUsers)
            {
                if (user.BoardGamePref != null)
                {
                    userGames = user.BoardGamePref;
                    gameList = GetBoardGames(userGames);

                    foreach (string game in gameList)
                    {
                        if (game == gameTitle)
                        {
                            matchList.Add(user);
                        }
                    }
                }
            }

            return matchList;
        }

        // Manipulates the data passed in the view to a string to be saved in the database
        public string SetBoardGames(List<string> boardGameList)
        {
            string boardGameString = null;

            for (int i = 0; i < boardGameList.Count; i++)
            {
                if (i == 0)
                {
                    boardGameString += boardGameList[i];
                }
                else
                {
                    boardGameString += (',' + boardGameList[i]);
                }
            }

            return boardGameString;
        }

        // Manipulates the string stored into the database into a list for user comparison
        public List<string> GetBoardGames(string boardGameString)
        {
            List<string> boardGameList = boardGameString.Split(',').ToList<string>();

            return boardGameList;
        }

        public List<string> GetMatches(AspNetUsers currentUser)
        {
            GamerMatchContext db = new GamerMatchContext();
            List<string> userList = new List<string>();
            List<MatchTable> matchList = db.MatchTable.ToList<MatchTable>();

            foreach (MatchTable match in matchList)
            {
                if (match.UserSend == currentUser.Id)
                {
                    if (match.Status == 1)
                    {
                        userList.Add(match.UserGet);
                    }
                }
            }

            return userList;
        }

        public List<string> GetBans(AspNetUsers currentUser)
        {
            GamerMatchContext db = new GamerMatchContext();
            List<string> userList = new List<string>();
            List<MatchTable> matchList = db.MatchTable.ToList<MatchTable>();

            foreach (MatchTable match in matchList)
            {
                if (match.UserSend == currentUser.Id)
                {
                    if (match.Status == 2)
                    {
                        userList.Add(match.UserGet);
                    }
                }
            }

            return userList;
        }
    }
}
