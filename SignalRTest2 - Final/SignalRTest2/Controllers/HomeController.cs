using Microsoft.AspNet.SignalR;
using MvcMobile.Models;
using OpenMetaverse;
using SignalRChat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;
using Textosecondlife.Models;

namespace SignalRTest2.Controllers
{
    public class HomeController : Controller
    {
        public static Dictionary<string, GridClient> Clients = new Dictionary<string, GridClient>();

        public ActionResult Chat()
        {
            return View();
        }

        public ActionResult Index(string returnUrl, string myUserId)
        {
            if (myUserId != null)
            {
                var x = Clients.FirstOrDefault(client => client.Key == myUserId);
                x.Value.Network.Logout();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Index(SecondLifeLogInModel model, string returnUrl)
        {

            string[] username = model.SlUsername.Split(new char[0]);
            string[] firstAndLastName = model.SlUsername.Split(new char[0]);

            if (username.Length < 2)
            {
                firstAndLastName = new string[2];
                firstAndLastName[0] = username[0];
                firstAndLastName[1] = "Resident";
            }
            else
            {
                firstAndLastName = username;
            }

            GridClient IAmAsAClient = new GridClient();

            if (IAmAsAClient.Network.Login(firstAndLastName[0], firstAndLastName[1], model.SlPassword, "IMToSecondlife", "1.0"))
            {
                var myId = IAmAsAClient.Self.AgentID.ToString();
                if (Clients.ContainsKey(myId))
                {
                    Clients.Remove(myId);
                }
                Clients.Add(myId, IAmAsAClient);

                System.Threading.Thread.Sleep(600);
                HomeModel results = new HomeModel();
                results = RefreshFriendList(IAmAsAClient);
                IAmAsAClient.Self.RetrieveInstantMessages();
                ReceiveIms(IAmAsAClient);
                ViewBag.OnlineUsers = results.OnlineUsers;
                ViewBag.OfflineUsers = results.OfflineUsers;
                results.MyUserUiid = myId;
                @ViewBag.dddd = myId;
                ViewBag.Username = model.SlUsername;
                return View("SecondLifeLogIn", results);
            }
            else
            {
                ModelState.AddModelError("", "Unable to Login: " + IAmAsAClient.Network.LoginMessage);
                return View("Index");
            }
        }

        public ActionResult FriendList(string myUserId)
        {
            if (myUserId == null)
            {
                ModelState.AddModelError("", "Please login before accessing your Friendlist");
                return View("Index");
            }
            GridClient IAmAsAClient = Clients[myUserId];
            @ViewBag.dddd = myUserId;
            HomeModel results = new HomeModel();
            results = RefreshFriendList(IAmAsAClient);
            return View("SecondLifeLogIn", results);

        }


        private HomeModel RefreshFriendList(GridClient ClientA)
        {
            HomeModel newModel = new HomeModel();
            newModel.OnlineUsers = new List<Friend>();
            newModel.OfflineUsers = new List<Friend>();

            ClientA.Friends.FriendList.ForEach(delegate(FriendInfo friend)
            {
                // append the name of the friend to our output

                if (friend.IsOnline)
                {
                    Friend onlineFriend = new Friend();
                    onlineFriend.FriendName = friend.Name;
                    onlineFriend.Uiid = friend.UUID.ToString();
                    onlineFriend.MyUserUiid = ClientA.Self.AgentID.ToString();
                    newModel.OnlineUsers.Add(onlineFriend);
                }

                else
                {
                    Friend offlineFriend = new Friend();
                    offlineFriend.FriendName = friend.Name;
                    offlineFriend.Uiid = friend.UUID.ToString();
                    offlineFriend.MyUserUiid = ClientA.Self.AgentID.ToString();
                    newModel.OfflineUsers.Add(offlineFriend);
                }
            });
            return newModel;
        }

        public void ReceiveIms(GridClient ClientA)
        {
            ClientA.Self.IM += new EventHandler<InstantMessageEventArgs>(Self_IM);
        }

        public static void Self_IM(object sender, InstantMessageEventArgs e)
        {
            if (e.IM.Dialog == InstantMessageDialog.MessageFromAgent)
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                hubContext.Clients.Group(e.IM.ToAgentID.ToString()).addChatMessage(e.IM.FromAgentName, e.IM.Message, e.IM.FromAgentID.ToString());
                HomeModel model = new HomeModel()
                {
                    slFirstName = e.IM.ToAgentID.ToString(),
                    slLastName = "",
                    FromUsername = e.IM.FromAgentName,
                    Message = e.IM.Message
                };
                sendEmail(model);
            }
        }


        public ActionResult StartChat(string userUid, string toUsername, string myUserID)
        {
            InstanteMessage session = new InstanteMessage();

            session.UserUiid = userUid;
            session.ToUsername = toUsername;
            session.MyUserUiid = myUserID;
            @ViewBag.dddd = myUserID;
            return View("StartChat", session);
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static string Donate()
        {
            return "Hello";
        }

        public void Donate(string userUid, string money, string myUserID)
        {
            if (money != null)
            {
                GridClient IAmAsAClient = Clients[myUserID];

                UUID cobaId = new UUID();
                cobaId.Guid = Guid.Parse("1a1338fb-e172-40c9-b5b7-de2fe5b06f99");

                IAmAsAClient.Self.GiveMoney(cobaId, Int32.Parse(money), "Donation", MoneyTransactionType.Gift, TransactionFlags.None);
                IAmAsAClient.Self.InstantMessage(cobaId, IAmAsAClient.Self.Name + " ti ha donato " + money + "L$");
                @ViewBag.dddd = myUserID;
            }
        }

        public void sendText(string message, string userUid, string myUserId)
        {
            GridClient IAmAsAClient = Clients[myUserId];

            UUID id = new UUID();
            id.Guid = Guid.Parse(userUid);
            IAmAsAClient.Self.InstantMessage(id, message);
            HomeModel model = new HomeModel()
            {
                slFirstName = userUid,
                slLastName = "",
                FromUsername = IAmAsAClient.Self.FirstName + " " + IAmAsAClient.Self.LastName,
                Message = message
            };
            sendEmail(model);
        }
       
        public ActionResult About(string myUserId)
        {
            @ViewBag.dddd = myUserId;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public PartialViewResult SendMessage()
        {
            return PartialView("SendMessage");
        }

        [HttpPost]
        public ActionResult SecondLifeLogIn(HomeModel model, string returnUrl)
        {
            string html = new WebClient().DownloadString("http://name2key.haxworx.net/?name=" + model.slFirstName + "+" + model.slLastName);
            var newModel = new HomeModel();
            if (ModelState.IsValid)
            {
                if (html == "00000000-0000-0000-0000-000000000000")
                {
                    ModelState.AddModelError("", "The Username entered does not exist. Please check the first and last name!");
                    foreach (var key in ModelState.Keys.Where(m => m.StartsWith("Mes")).ToList())
                        ModelState.Remove(key);
                    return View("SendMessage", model);
                }
                else
                    sendEmail(model);
                ViewData["Message"] = "Success";
            }
            else
                ModelState.AddModelError("", "Please fill in the message box!");
            foreach (var key in ModelState.Keys.Where(m => m.StartsWith("Mes")).ToList())
                ModelState.Remove(key);

            var list = (List<string>)Session["test"];

            ViewBag.MyList = list;

            return View("SendMessage", model);

        }

        public static void sendEmail(HomeModel model)
        {
            HomeModel user = new HomeModel();
            string primKey = ConfigurationManager.AppSettings["primKey"];
            var fromAddress = new MailAddress("AGmailAddress@gmail.com", "From Name");
            var toAddress = new MailAddress(primKey + "@lsl.secondlife.com", "To Name");

            const string fromPassword = "gmailpassword";
            string subject = model.slFirstName + " " + model.slLastName;
            string body = model.FromUsername + " END " + model.Message + " User IP: " + model.userIP;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
