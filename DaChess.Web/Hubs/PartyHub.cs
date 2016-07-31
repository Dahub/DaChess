using Microsoft.AspNet.SignalR;

namespace DaChess.Web.Hubs
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

        public void NewMove(string partyName)
        {
            Clients.Group(partyName).moveOver();
        }

        public void NewInfo(string partyName, string info)
        {
            Clients.Group(partyName).sendInfo(info);
        }

        public void APlayerResign(string partyName)
        {
            Clients.Group(partyName).playerResign();
        }

        public void AskForDrawn(string partyName)
        {
            Clients.Group(partyName).playerAskDrawn();
        }

        public void APlayerAcceptDrawn(string partyName, string token)
        {
            Clients.Group(partyName).playerAcceptDrawn(token);
        }
    }
}