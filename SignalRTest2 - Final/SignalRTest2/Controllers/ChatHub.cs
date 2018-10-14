using Microsoft.AspNet.SignalR;
using SignalRTest2.Controllers;

namespace SignalRChat
{
    public class ChatHub : Hub
    {

        public void Send(string name, string message, string userUid, string myUserId)
        {
            Groups.Add(Context.ConnectionId, myUserId);
            HomeController IM = new HomeController();
            IM.sendText(message, userUid, myUserId);
        }

        public void Send(string myUserId)
        {
            Groups.Add(Context.ConnectionId, myUserId);
        }

        public void AddToGroup(string myUserId)
        {
            Groups.Add(Context.ConnectionId, myUserId);
        }


        //public override Task OnDisconnected(bool stopCalled)
        //{

            
        //    return base.OnDisconnected(stopCalled);
        //}
    }
}