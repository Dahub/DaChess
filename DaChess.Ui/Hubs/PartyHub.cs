using Microsoft.AspNet.SignalR;

namespace DaChess.Ui.Hubs
{
    public class PartyHub : Hub
    {
        public void JoinParty(string partyName)
        {
            Groups.Add(Context.ConnectionId, partyName);
        }

        public void SendMessage(string partyName, string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.Group(partyName).addNewMessageToPage(name, message);
        }
    }
}