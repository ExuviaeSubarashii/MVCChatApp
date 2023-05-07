using Microsoft.AspNetCore.Mvc;
using MVC.Domain.Models;
using MVC.Domain.SupClass;
using MVC.Service.Extension;
using MVCChatApp.Models;
using System.Diagnostics;

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
        public ActionResult Login(User user)
        {
            if (user.Username != null && user.Password != null)
            {
                var query = _CP.Users.Any(x => x.Username.TrimEnd() == user.Username.TrimEnd() & x.Password.TrimEnd() == user.Password.TrimEnd());

                if (query)
                {
                    AppMain.User = new User();
                    AppMain.User.Username = user.Username;
                    return View("Privacy");
                }
                else
                {
                    return NotFound();
                }
            }
            return Ok();
        }
        [HttpPost]
        public ActionResult RegisterAccount(User user) 
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
                return RedirectToAction("Index");
            }
        }
       [HttpPost]
       public ActionResult ChooseServer()
        {
            if (AppMain.User.Username!=null)
            {
                var query = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
                return View(query.ToList());
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ViewResult ReturnChatScreen(string channelButton)
        {
            var query = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == channelButton.Trim() && x.ImageDir != "Null").ToList();
            return View("ChatMainScreen", query.ToList());
        }

        [HttpPost]
        public ActionResult GetAllChannels(string serverButton)
        {
            var query = _CP.Servers.Where(x => x.ServerName.Trim() == serverButton.Trim()).ToList();
            AppMain.User.Server=serverButton.Trim();
            return View("ChooseChannel",query);
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