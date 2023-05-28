using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using MVC.Domain.Models;
using MVC.Domain.SupClass;
using MVC.Service.Extension;
using MVCChatApp.Models;
using Serilog;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MVCChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ChatContext _CP;
        public HomeController(ILogger<HomeController> logger, ChatContext CP)
        {
            _logger = logger;
            _CP = CP;
        }
        [HttpPost]
        public async Task<ActionResult> Login(User user)
        {
            if (user.Username != null && user.Password != null)
            {
                var checkUserExists = _CP.Users.Any(x => x.Username.TrimEnd() == user.Username.TrimEnd() & x.Password.TrimEnd() == user.Password.TrimEnd());

                if (checkUserExists)
                {
                    AppMain.User = new User();
                    AppMain.User.Username = user.Username;
                    var checkserverQuery = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
                    await Task.Delay(1000);
                    return View("ChooseServer", checkserverQuery.ToList());
                }
                else
                {
                    var log = new LoggerConfiguration()
                    .WriteTo.File("UserLogs/UserFailedLogs.txt")
                    .CreateLogger();
                    log.Information($"User {AppMain.User.Username}"+
                    "Failed Login.");
                    log.Information("------------------------------------------------------------");
                    return NotFound();
                }
            }
            await Task.Delay(1000);
            return Ok();
        }
        [HttpPost]
        public async Task<ActionResult> RegisterAccount(User user)
        {
            var query = _CP.Users.Where(x => x.Username == user.Username).Any();
            User newUser = new User()
            {
                Username = user.Username,
                Password = user.Password,
                HasPassword = user.Password.ConvertStringToMD5(),
                Image = null,
                Server = "Main"
            };
            if (query == true)
            {
                return StatusCode(500);
            }
            else if (user.Username == null || user.Username == "" || string.IsNullOrEmpty(user.Username))
            {
                return StatusCode(406);
            }
            else
            {
                _CP.Users.Add(newUser);
                _CP.SaveChanges();
                await Task.Delay(1000);
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult GetAllChannels(string serverButton)
        {
            AppMain.User.Server = serverButton.Trim();
            var query = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).ToList();
            return View("ChooseChannel", query);
        }

        [HttpPost]
        public ViewResult ReturnChatScreen(string channelButton)
        {
            var query = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == channelButton.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            AppMain.Servers.Channels = channelButton.ToString();
            return View("ChatMainScreen", query.ToList());
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(string messageInput)
        {
            var query = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == AppMain.Servers.Channels.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            Message newmsg = new Message()
            {
                Message1 = messageInput,
                SenderName = AppMain.User.Username,
                SenderTime = DateTime.UtcNow,
                Server = AppMain.User.Server,
                Channel = AppMain.Servers.Channels,
                ImageDir = null,
            };
            if (messageInput == null)
            {
                return View("ChatMainScreen", query.ToList());
            }
            _CP.Messages.Add(newmsg);
            _CP.SaveChanges();
            var queryaftersending = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == AppMain.Servers.Channels.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            await Task.Delay(1000);
            return View("ChatMainScreen", queryaftersending);
        }
        #region CreateNewServer
        [HttpPost]
        public async Task<ActionResult> CreateNewServer(string newserverInput)
        {
            Server servers1 = new Server()
            {
                Usernames = AppMain.User.Username,
                Channels = "Main",
                ServerName = newserverInput,
                ServerOwner = AppMain.User.Username
            };
            var loadthisifitfails = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            var checkifserverExists = _CP.Servers.Any(x => x.ServerName.TrimEnd() == newserverInput.TrimEnd());
            var theserverwilladd = _CP.Users.Where(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd()).FirstOrDefault();
            if (checkifserverExists)
            {
                return View("ChooseServer", loadthisifitfails);
            }
            theserverwilladd.Server = theserverwilladd.Server.TrimEnd() + ", " + newserverInput.TrimEnd();
            _CP.Servers.Add(servers1);
            _CP.SaveChanges();
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("ChooseServer", query3.ToList());
        }
        #endregion
        #region JoinServer - first checks if the servers exists, if turns true it will add to the database if it does not exists it will return back to ChooseServer 
        [HttpPost]
        public async Task<ActionResult> JoinServer(string newserverInput)
        {
            //first checks if the servers exists, if turns true it will add to the database if it does not exists it will return back to ChooseServer page.
            string[] sww = null;
            //var checkifUserExists = _CP.Users.Any(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd());
            var returnthisWhenServerDoesNotExist = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            var checkifUServerExists = _CP.Servers.Any(x => x.ServerName.TrimEnd() == newserverInput.TrimEnd());
            var theserverwilladd = _CP.Users.Where(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd()).FirstOrDefault();
            var checkIfUserIsAlreadyInThisServer = _CP.Users.Any(x => x.Username.Trim() == AppMain.User.Username.Trim() && x.Server.Contains(newserverInput.Trim()));
            if (checkifUServerExists)
            {
                if (!checkIfUserIsAlreadyInThisServer)
                {
                    theserverwilladd.Server = theserverwilladd.Server.TrimEnd() + ", " + newserverInput.TrimEnd();
                    var log = new LoggerConfiguration()
                    .WriteTo.File($"ServerLogs/{newserverInput} Logs.txt")
                    .CreateLogger();
                    log.Information($"User {AppMain.User.Username} Joined at {DateTime.Now}.");
                    log.Information("------------------------------------------------------------");
                }
            }
            else
            {
                return View("ChooseServer", returnthisWhenServerDoesNotExist);
            }
            _CP.SaveChanges();
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("ChooseServer", query3.ToList());
        }
        #endregion
        #region CreateNewChannel - check first if user is the owner of the server and adds If not it will turn back.
        [HttpPost]
        public async Task<ActionResult> CreateNewChannel(string newchannelInput)
        {
            //check first if user is the owner of the server and adds If not it will turn back.
            var whenqueryisfalse = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).ToList();
            if (newchannelInput == null)
            {
                return View("ChooseChannel", whenqueryisfalse);
            }
            var checkifuserisowner = _CP.Servers.Any(x => x.ServerName.Trim() == AppMain.User.Server.Trim() && x.ServerOwner == AppMain.User.Username);
            var channelswillbeadded = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).FirstOrDefault();

            if (checkifuserisowner)
            {
                channelswillbeadded.Channels = channelswillbeadded.Channels.TrimEnd() + "," + newchannelInput.TrimEnd();
                var log = new LoggerConfiguration()
                    .WriteTo.File($"ServerLogs/{AppMain.User.Server} Logs.txt")
                    .CreateLogger();
                log.Information($"Channel Name {newchannelInput} Created at {DateTime.Now}.");
                log.Information("------------------------------------------------------------");
            }
            else
            {
                var log = new LoggerConfiguration()
                   .WriteTo.File($"ServerLogs/{AppMain.User.Server} Logs.txt")
                   .CreateLogger();
                log.Information($"Failed to Create Channel Name {newchannelInput} at {DateTime.Now}.");
                log.Information("------------------------------------------------------------");
                return View("ChooseChannel", whenqueryisfalse);
            }
            _CP.SaveChanges();
            var query5 = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).ToList();
            return View("ChooseChannel", query5);
        }
        #endregion
        [HttpPost]
        public async Task<ActionResult> GetBackToChoosingServer()
        {
            var checkserverQuery = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("ChooseServer", checkserverQuery.ToList());
        }
        public ActionResult LogOut()
        {
            AppMain.User.Username = "";
            return View("Index");
        }
        [HttpPost]
        public async Task<ActionResult> SendFriendRequest(string friendName)
        {
            var checkifUserExists = _CP.Users.Any(x => x.Username.Trim() == friendName.Trim());
            var checkIfUserIsAlreadyFriend = _CP.Users.Any(x => x.Username.Trim() == AppMain.User.Username.Trim() && x.Friends.Contains(friendName.Trim()));
            var willAddtoFriendList = _CP.Users.Where(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd()).FirstOrDefault();
            if (checkifUserExists&& !checkIfUserIsAlreadyFriend&&friendName!=null)
            {
               willAddtoFriendList.Friends = willAddtoFriendList.Friends.Trim() + "," + friendName.Trim();
            }
            else
            {
                await Task.Delay(1000);
                return View("AddNewFriend");
            }
            _CP.SaveChanges();
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("AddNewFriend",query3);
        }
        [HttpPost]
        public async Task<ActionResult> AddFriend()
        {
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("AddNewFriend", query3);
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}