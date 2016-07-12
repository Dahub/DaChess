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
            Clients.Group(partyName).addNewMessageToPage(name, message);
        }

        public void SendAddWhitePlayer(string partyName)
        {
            Clients.Group(partyName).addPlayerWhite();
        }

        public void SendAddBlackPlayer(string partyName)
        {
            Clients.Group(partyName).addPlayerBlack();
        }
    }
}