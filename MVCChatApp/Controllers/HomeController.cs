using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using MVC.Domain.Models;
using MVC.Domain.SupClass;
using MVC.Service.Extension;
using MVCChatApp.Models;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(User user)
        {
            if (string.IsNullOrEmpty(user.EMail) || string.IsNullOrEmpty(user.Password))
            {
                return View("Index");
            }

            var checkUserExists = _CP.Users.Any(x => x.EMail.TrimEnd() == user.EMail.TrimEnd() && x.Password.TrimEnd() == user.Password.TrimEnd());
            var isthistheuser=_CP.Users.Where(x => x.EMail.TrimEnd() == user.EMail.TrimEnd() && x.Password.TrimEnd() == user.Password.TrimEnd()).FirstOrDefault();
            if (checkUserExists)
            {
                AppMain.User = new User { Username = isthistheuser.Username , EMail = user.EMail};
                var checkserverQuery = _CP.Users.Where(x => x.EMail.Trim() == AppMain.User.EMail).ToList();
                return View("ChooseServer", checkserverQuery.ToList());
            }
            else
            {
                var log = new LoggerConfiguration()
                    .WriteTo.File("UserLogs/UserFailedLogs.txt")
                    .CreateLogger();
                log.Information($"User {user.Username} Failed Login.");
                log.Information("------------------------------------------------------------");
                return NotFound();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAccount(User user)
        {
            var query = _CP.Users.Where(x => x.EMail == user.EMail).Any();
            User newUser = new User()
            {
                Username = user.Username,
                Password = user.Password,
                EMail = user.EMail,
                HasPassword = user.Password.ConvertStringToMD5(),
                Image = null,
                Server = "Main"
            };
            if (query == true)
            {
                return View("RegisterPage");
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
        public ActionResult RegisterPage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetAllChannels(string serverButton)
        {
            var query = _CP.Servers.Where(x => x.ServerName.Trim() == serverButton.Trim()).ToList();
            AppMain.User.Server = serverButton.Trim();
            return View("ChooseChannel",query);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ViewResult ReturnChatScreen(string channelButton)
        {
            //var query = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == channelButton.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            if (channelButton == null)
            {
                channelButton = AppMain.Servers.Channels;
            }
            AppMain.Servers.Channels = channelButton;
            return View("ChatMainScreen"/*, query.ToList()*/);
        }
        [HttpGet]
        public async Task<ActionResult> ReloadChatScreen()
        {
            var query = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == AppMain.Servers.Channels.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            //await Task.Delay(3000);
            //var jsonString = JsonSerializer.Serialize(query);
            return Json(query);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
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
                return Json(query);
            }
            _CP.Messages.Add(newmsg);
            _CP.SaveChanges();
            var queryaftersending = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == AppMain.Servers.Channels.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            await Task.Delay(1000);
            return Ok();
        }
        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetAllMessages()
        {
            var queryaftersending = _CP.Messages.Where(x => x.Server == AppMain.User.Server.Trim() && x.Channel == AppMain.Servers.Channels.Trim() && x.ImageDir != "Null").OrderByDescending(x => x.Id).ToList();
            return Ok(queryaftersending);
        }
        #region CreateNewServer
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> JoinServer(string newserverInput)
        {
            //first checks if the servers exists, if turns true it will add to the database if it does not exists it will return back to ChooseServer page.
            string[] sww = null;
            //var checkifUserExists = _CP.Users.Any(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd());
            var returnthisWhenServerDoesNotExist = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            var checkifUServerExists = _CP.Servers.Any(x => x.ServerName.TrimEnd() == newserverInput.TrimEnd());
            var theserverwilladd = _CP.Users.Where(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd()).FirstOrDefault();
            var checkIfUserIsAlreadyInThisServer = _CP.Users.Any(x => x.Username.Trim() == AppMain.User.Username.Trim() && x.Server.Contains(newserverInput.Trim()));
            if (checkifUServerExists==false)
            {
                if (checkIfUserIsAlreadyInThisServer==false)
                {
                    theserverwilladd.Server = theserverwilladd.Server.TrimEnd() + ", " + newserverInput.TrimEnd();
                    var log = new LoggerConfiguration()
                    .WriteTo.File($"ServerLogs/{newserverInput} Logs.txt")
                    .CreateLogger();
                    log.Information($"User {AppMain.User.Username} Joined at {DateTime.Now}.");
                    log.Information("------------------------------------------------------------");
                    _CP.SaveChanges();
                }
            }
            else
            {
                return View("ChooseServer", returnthisWhenServerDoesNotExist);
            }
            
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("ChooseServer", query3.ToList());
        }
        #endregion
        #region CreateNewChannel - check first if user is the owner of the server and adds If not it will turn back.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetBackToChoosingServer()
        {
            var checkserverQuery = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            //await Task.Delay(1000);
            return View("ChooseServer", checkserverQuery.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOut()
        {
            AppMain.User.Username = "";
            return View("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendFriendRequest(string friendName)
        {
            if (friendName != null)
            {
                var checkifUserExists = _CP.Users.Any(x => x.Username.Trim() == friendName.Trim());
                var checkIfUserIsAlreadySentFriendRequest = _CP.Users.Any(x => x.Username.Trim() == AppMain.User.Username.Trim() && x.FriendRequests.Contains(friendName.Trim()));
                var checkIfUserIsAlreadyFriend = _CP.Users.Any(x => x.Username.Trim() == AppMain.User.Username.Trim() && x.Friends.Contains(friendName.Trim()));
                var willAddtoFriendList = _CP.Users.Where(x => x.Username.TrimEnd() == friendName.TrimEnd()).FirstOrDefault();
                if (checkifUserExists && !checkIfUserIsAlreadyFriend && !checkIfUserIsAlreadySentFriendRequest && friendName != null)
                {
                    willAddtoFriendList.FriendRequests = willAddtoFriendList.FriendRequests.Trim() + "," + AppMain.User.Username.Trim();
                }
                else
                {
                    //await Task.Delay(1000);
                    var query = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();                    
                    return View("AddNewFriend",query);
                }
                _CP.SaveChanges();
            }

            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("AddNewFriend", query3);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route($"/Home")]
        public async Task<ActionResult> Home()
        {
            var query3 = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            //await Task.Delay(1000);
            return View("AddNewFriend", query3);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AcceptFriendRequest(string friendName)
        {
            if (friendName != null)
            {
                //kabul edenin tarafinda ekle
                var willAddtoFriendList = _CP.Users.Where(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd()).FirstOrDefault();
                willAddtoFriendList.Friends = willAddtoFriendList.Friends.Trim() + "," + friendName.Trim();
                _CP.SaveChanges();
                //remove the friendrequest from the database
                string[] acceptedfriendRequest = null;
                List<string> sw = new List<string>();

                var query = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).FirstOrDefault();
                var query2 = _CP.Users.Where(x => x.Username == AppMain.User.Username).ToList();
                foreach (var item in query2)
                {
                    acceptedfriendRequest = item.FriendRequests.Split(',');
                }
                List<String> list = acceptedfriendRequest.ToList();
                list.Remove(friendName);
                string[] columns = list.ToArray();
                var newfriendList = string.Join(",", columns);
                query.FriendRequests = newfriendList;
                _CP.SaveChanges();

                //gonderenin tarafinda ekle
                var senderwillAddtoFriendList= _CP.Users.Where(x => x.Username.TrimEnd() == friendName.Trim()).FirstOrDefault();

                senderwillAddtoFriendList.Friends = senderwillAddtoFriendList.Friends.Trim() + "," + AppMain.User.Username.Trim();

                _CP.SaveChanges();
                //remove the friendrequest from the database by sender side
                string[] senderacceptedfriendRequest = null;
                List<string> sw2 = new List<string>();

                var query3 = _CP.Users.Where(x => x.Username.Trim() == friendName.Trim()).FirstOrDefault();
                var query4 = _CP.Users.Where(x => x.Username == friendName.Trim()).ToList();

                foreach (var item in query4)
                {
                    senderacceptedfriendRequest = item.FriendRequests.Split(',');
                }

                List<String> senderlist = senderacceptedfriendRequest.ToList();
                list.Remove(AppMain.User.Username);

                string[] sendercolumns = list.ToArray();
                var sendernewfriendList = string.Join(",", sendercolumns);
                query3.FriendRequests = sendernewfriendList;
                _CP.SaveChanges();
            }
            var refreshFriendPage = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
                //await Task.Delay(1000);
            return View("AddNewFriend", refreshFriendPage);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeclineFriendRequest(string friendName)
        {
            if (friendName != null)
            {
                string[] acceptedfriendRequest = null;
                List<string> sw = new List<string>();
                var query = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).FirstOrDefault();
                var query2 = _CP.Users.Where(x => x.Username == AppMain.User.Username).ToList();
                foreach (var item in query2)
                {
                    acceptedfriendRequest = item.FriendRequests.Split(',');
                }
                List<String> list = acceptedfriendRequest.ToList();
                list.Remove(friendName);
                string[] columns = list.ToArray();
                var newfriendList = string.Join(",", columns);
                query.FriendRequests = newfriendList;
                _CP.SaveChanges();
            }
            var refreshFriendPage = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            //await Task.Delay(1000);
            return View("AddNewFriend", refreshFriendPage);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BlockUser(string friendName)
        {
            if (friendName != null)
            {
                //remove the user from friendlist
                string[] currentFriendList = null;
                List<string> sw = new List<string>();
                var query = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).FirstOrDefault();
                var query2 = _CP.Users.Where(x => x.Username == AppMain.User.Username).ToList();
                foreach (var item in query2)
                {
                    currentFriendList = item.Friends.Split(',');
                }
                List<String> list = currentFriendList.ToList();
                list.Remove(friendName);
                string[] columns = list.ToArray();
                var newfriendList = string.Join(",", columns);
                query.Friends = newfriendList;
                _CP.SaveChanges();

                //now add the blocked user to BlockedUser slot
                var willAddtoBlockedList = _CP.Users.Where(x => x.Username.TrimEnd() == AppMain.User.Username.TrimEnd()).FirstOrDefault();
                willAddtoBlockedList.BlockedUsers = willAddtoBlockedList.BlockedUsers.Trim() + "," + friendName.Trim();
                _CP.SaveChanges();
            }
            var refreshFriendPage = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            //await Task.Delay(1000);
            return View("AddNewFriend", refreshFriendPage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FriendChatPanel(string friendName)
        {
            string currentUser = AppMain.User.Username;
            AppMain.DirectMessages = new MVC.Domain.Models.DirectMessages
            {
                ReceiverName = friendName.Trim()
            };
            string receiverName = friendName.Trim();

            var directMessagesForChat = _CP.DirectMessages
                .Where(x => (x.SenderName == currentUser && x.ReceiverName == receiverName) ||
                            (x.ReceiverName == currentUser && x.SenderName == receiverName))
                .OrderByDescending(x => x.Id)
                .ToList();

            //await Task.Delay(1000);
            return View("FriendChatPanel");
        }
        
        [HttpPost]
        public async Task<ActionResult> SendDirectMessage(string messageInput)
        {
            string currentUser = AppMain.User.Username;
            string receiverName = AppMain.DirectMessages.ReceiverName.Trim();

            var directMessagesBeforeSending = GetDirectMessages(currentUser, receiverName);

            if (messageInput == null)
            {
                return Ok();
            }

            DirectMessages newMessage = new DirectMessages()
            {
                Message = messageInput.Trim(),
                MessageDate = DateTime.Now,
                ReceiverName = receiverName,
                SenderName = currentUser,
            };

            _CP.DirectMessages.Add(newMessage);
            _CP.SaveChanges();

            var directMessagesAfterSending = GetDirectMessages(currentUser, receiverName);

            await Task.Delay(100);
            return new JsonResult(directMessagesAfterSending);
        }

        private List<DirectMessages> GetDirectMessages(string senderName, string receiverName)
        {
            return _CP.DirectMessages
                .Where(x => (x.SenderName == senderName && x.ReceiverName == receiverName) ||
                            (x.ReceiverName == senderName && x.SenderName == receiverName))
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        [HttpGet]
        public async Task<ActionResult> ReloadFriendChatPanel()
        {
            string currentUser = AppMain.User.Username;
            var directMessagesForChat = _CP.DirectMessages
                .Where(x => (x.SenderName == currentUser && x.ReceiverName == AppMain.DirectMessages.ReceiverName) ||
                            (x.ReceiverName == currentUser && x.SenderName == AppMain.DirectMessages.ReceiverName))
                .OrderByDescending(x => x.Id)
                .ToList();
            return Json(directMessagesForChat);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveFromFriendList(string friendName)
        {
            if (friendName != null)
            {
                string[] currentFriendList = null;
                List<string> sw = new List<string>();
                var query = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).FirstOrDefault();
                var query2 = _CP.Users.Where(x => x.Username == AppMain.User.Username).ToList();
                foreach (var item in query2)
                {
                    currentFriendList = item.Friends.Split(',');
                }
                List<String> list = currentFriendList.ToList();
                list.Remove(friendName);
                string[] columns = list.ToArray();
                var newfriendList = string.Join(",", columns);
                query.Friends = newfriendList;
                _CP.SaveChanges();
            }
            var refreshFriendPage = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            await Task.Delay(1000);
            return View("AddNewFriend", refreshFriendPage);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMessage(int messageToDelete)
        {
            
            var deleteMessageQuery = _CP.Messages.Where(x => x.Id == messageToDelete).FirstOrDefault();
            if (deleteMessageQuery.SenderName.Trim()==AppMain.User.Username)
            {
                _CP.Messages.Remove(deleteMessageQuery);
                _CP.SaveChanges();
                return Ok();
            }
            else
            {
                return Ok();
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDirectMessage(int messageToDelete)
        {
            var deleteMessageQuery = _CP.DirectMessages.Where(x => x.Id == messageToDelete).FirstOrDefault();
            if (deleteMessageQuery.SenderName.Trim()==AppMain.User.Username)
            {
                _CP.DirectMessages.Remove(deleteMessageQuery);
                _CP.SaveChanges();
                return Ok();
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllServers()
        {
            var checkserverQuery = _CP.Users.Where(x => x.Username.Trim() == AppMain.User.Username).ToList();
            return Json(checkserverQuery);
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