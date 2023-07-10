using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GamerMatch.Models;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace GamerMatch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _config;
        private ApiController apiController;
        private DatabaseController databaseController;
        private MatchController matchController;
        public GamerMatchContext gc = new GamerMatchContext();
        private AspNetUsers currentUser = new AspNetUsers();

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            apiController = new ApiController(config);
            databaseController = new DatabaseController(config);
            matchController = new MatchController(config);
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.Name != null)
            {
                await FindUser();
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> HomePage()
        {
            await FindUser();

            return View(currentUser);
        }

        public async Task<IActionResult> Preferences()
        {
            await FindUser();

            return View(currentUser);
        }

        public async Task<IActionResult> UserPerf(string steam, string difficulty, string[] boardgames)
        {
            await FindUser();

            List<string> myGames = boardgames.ToList<string>();
            currentUser.BoardGamePref = databaseController.SetBoardGames(myGames);
            currentUser.UserPref = difficulty;

            if (steam == null)
            {
                gc.SaveChanges();
                return Redirect("HomePage");
            }
            else
            {
                if (await apiController.ValidateSteamID(steam))
                {
                    currentUser.SteamInfo = steam;
                    foreach (var user in gc.AspNetUsers)
                    {
                        if (user.UserName == currentUser.UserName)
                        {
                            user.SteamInfo = steam;
                            currentUser.SteamInfo = steam;
                        }
                    }
                    gc.SaveChanges();
                    ViewData["MyGames"] = await apiController.GetSteamLibrary(currentUser.SteamInfo);
                    return Redirect("HomePage");
                }
                else
                {
                    steam = null;
                    foreach (var user in gc.AspNetUsers)
                    {
                        if (user.UserName == currentUser.UserName)
                        {
                            user.SteamInfo = steam;
                            currentUser.SteamInfo = steam;
                            ViewData["Error"] = "Invalid Steam ID.";
                        }
                    }

                    await FindUser();
                    gc.SaveChanges();
                    return View("Preferences", currentUser);
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task FindUser()
        {
            foreach (var person in gc.AspNetUsers)
            {
                if (person.UserName == User.Identity.Name)
                {
                    currentUser = person;
                }
            }

            try
            {
                if (currentUser.SteamInfo != null)
                {
                    ViewData["MyGames"] = await apiController.GetSteamLibrary(currentUser.SteamInfo);
                }
                else
                {
                    ViewData["MyGames"] = null;
                }
            }
            catch (KeyNotFoundException)
            {
                ViewData["MyGames"] = null;
            }

            ViewData["Games"] = gc.BoardGames.ToList<BoardGames>();
            ViewData["Users"] = gc.AspNetUsers.ToList<AspNetUsers>();
            ViewData["Friends"] = databaseController.GetMatches(currentUser);
            ViewData["Bans"] = databaseController.GetBans(currentUser);
        }

        public async Task<IActionResult> Results(string steamTitle, string boardTitle)
        {
            await FindUser();

            List<AspNetUsers> matchList = await databaseController.SearchSplit(steamTitle, boardTitle);
            List<AspNetUsers> displayList = new List<AspNetUsers>();
            List<MatchTable> matches = gc.MatchTable.ToList<MatchTable>();
            bool matched;

            foreach (AspNetUsers user in matchList)
            {
                matched = false;
                foreach (MatchTable match in matches)
                {
                    if (match.UserGet == user.UserName)
                    {
                        if (match.UserSend == currentUser.Id)
                        {
                            if (match.Status == 3)
                            {
                                if (!displayList.Contains(user))
                                {
                                    matched = true;
                                    displayList.Add(user);
                                }
                            }
                            else
                            {
                                matched = true;
                                continue;
                            }
                        }
                    }

                    if (user.UserName == match.UserGet && match.Status != 3)
                    {
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (matched == false && user.Id != currentUser.Id)
                {
                    displayList.Add(user);
                }
            }

            List<Result> resultList = await matchController.MatchTotalsList(displayList, currentUser);

            ViewData["Search"] = new List<string>
                {
                steamTitle,
                boardTitle
                };

            if (displayList.Count == 0)
            {
                ViewData["No"] = "No search results found.";
                return View();
            }
            else
            {
                return View(resultList);
            }
        }

        public async Task<IActionResult> Ratings()
        {
            await FindUser();

            List<MatchTable> displayList = new List<MatchTable>();
            List<MatchTable> matches = gc.MatchTable.ToList<MatchTable>();

            foreach (MatchTable match in matches)
            {
                if (match.UserSend == currentUser.Id && match.Status == 1 && match.Rating == null)
                {
                    displayList.Add(match);
                }
            }

            return View(displayList);
        }

        public async Task<IActionResult> Profile(string friend)
        {
            AspNetUsers profile = new AspNetUsers();

            await FindUser();

            foreach (AspNetUsers user in gc.AspNetUsers)
            {
                if (user.UserName == friend)
                {
                    profile = user;
                }
            }

            ViewData["Current"] = JsonSerializer.Serialize(currentUser);

            return View(profile);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

