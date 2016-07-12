using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace DaChess.SignalRClient
{
    // http://www.asp.net/signalr/overview/getting-started/tutorial-getting-started-with-signalr-and-mvc
    public class ChatHub : Hub
    {        
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            
            Clients.All.addNewMessageToPage(name, message);
        }
    }
}