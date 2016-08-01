using Microsoft.AspNet.SignalR;

namespace DaChessV2.Web.Hubs
{
    public class PartyHub : Hub
    {
        public void JoinParty(string partyName)
        {
            Groups.Add(Context.ConnectionId, partyName);
        }

        public void SendMessage(string partyName, string name, string message)
        {
            Clients.Group(partyName).addNewMessageToPage(name, message);
        }
    }
}