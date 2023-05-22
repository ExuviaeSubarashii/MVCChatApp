using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using MVC.Domain.Models;
using MVC.Domain.SupClass;
using MVC.Service.Extension;
using MVCChatApp.Models;
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
        public ActionResult ChooseServer()
        {
            if (AppMain.User.Username!=null)
            {
                var query = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
                return View("ChooseServer",query.ToList());
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost]
        public ViewResult ReturnChatScreen(string channelButton)
        {
            var query = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == channelButton.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            AppMain.Servers.Channels = channelButton.ToString();
            return View("ChatMainScreen", query.ToList());
        }
        [HttpPost]
        public ActionResult GetAllChannels(string serverButton)
        {
            AppMain.User.Server=serverButton.Trim();
            var query = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).ToList();
            return View("ChooseChannel",query);
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
            if (messageInput==null)
            {
                return View("ChatMainScreen", query.ToList());
            }
            _CP.Messages.Add(newmsg);
            _CP.SaveChanges();
            var queryaftersending = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == AppMain.Servers.Channels.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            await Task.Delay(1000);
            return View("ChatMainScreen", queryaftersending);
        }
        [HttpPost]
        public async Task<ActionResult> CreateNewServer(string serverInput) 
        {
            Server servers1 = new Server()
            {
                Usernames = AppMain.User.Username,
                Channels = "Main",
                ServerName = serverInput,
            };
            var checkifserverExists = _CP.Servers.Any(x => x.ServerName.TrimEnd() == serverInput.TrimEnd());
            if (checkifserverExists)
            {
                return NotFound();
            }
            _CP.Servers.Add(servers1);
            _CP.SaveChanges();
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("ChooseServer", query3.ToList());
        }
        [HttpPost]
        public async Task<ActionResult> JoinServer(string newserverInput)
        {
            string[] sww = null;
            var checkifUserExists = _CP.Users.Any(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd());
            var theserverwilladd = _CP.Users.Where(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd()).FirstOrDefault();
            if (checkifUserExists)
            {
                theserverwilladd.Server = theserverwilladd.Server.TrimEnd() + ", " + newserverInput.TrimEnd();
            }
            _CP.SaveChanges();
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("ChooseServer", query3.ToList());
        }
        [HttpPost]
        public async Task<ActionResult> CreateNewChannel(string newchannelInput)
        {
            var whenqueryisfalse = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).ToList();
            if (newchannelInput==null)
            {
                return View("ChooseChannel", whenqueryisfalse);
            }
            var checkifuserisowner = _CP.Servers.Any(x => x.ServerName.Trim() == AppMain.User.Server.Trim()&&x.ServerOwner==AppMain.User.Username);
            var channelswillbeadded = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).FirstOrDefault();
            
            if (checkifuserisowner)
            {
                channelswillbeadded.Channels = channelswillbeadded.Channels.TrimEnd() + "," + newchannelInput.TrimEnd();
            }
            else
            {
                return View("ChooseChannel", whenqueryisfalse);
            }
            _CP.SaveChanges();
            var query5 = _CP.Servers.Where(x => x.ServerName.Trim() == AppMain.User.Server.Trim()).ToList();
            return View("ChooseChannel", query5);
        }
        public ActionResult LogOut()
        {
            AppMain.User.Username ="";
            return View("Index");
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