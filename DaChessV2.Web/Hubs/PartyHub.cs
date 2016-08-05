using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using DaChessV2.Dto;

namespace DaChessV2.Web.Hubs
{
    public class PartyHub : Hub
    {
        public override Task OnDisconnected(bool stopCalled)
        {
            ClientInfo info = ClientsStack.Get(Context.ConnectionId);
            if (info.Color.HasValue && info.Color == Color.BLACK)
            {
                // info que le joueur des noirs est parti
                Clients.Group(info.PartyName).sendInfo("Le joueur noir s'est déconnecté");
            }
            else if (info.Color.HasValue && info.Color == Color.WHITE)
            {
                Clients.Group(info.PartyName).sendInfo("Le joueur blanc s'est déconnecté");
            }
            ClientsStack.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public void JoinParty(string partyName, bool isWhite, bool isBlack)
        {
            Color? userColor = null;

            if (isWhite)
            { 
                userColor = Color.WHITE;
                Clients.Group(partyName).sendInfo("Le joueur blanc s'est connecté");
            }
            if (isBlack)
            {
                userColor = Color.BLACK;
                Clients.Group(partyName).sendInfo("Le joueur noir s'est connecté");
            }

                ClientsStack.Add(Context.ConnectionId, partyName, userColor.HasValue, userColor);
            Groups.Add(Context.ConnectionId, partyName);
        }

        public void SendAddWhitePlayer(string partyName)
        {
            ClientsStack.Update(Context.ConnectionId, partyName, true, Dto.Color.WHITE);
            Clients.Group(partyName).addPlayerWhite();
        }

        public void SendAddBlackPlayer(string partyName)
        {
            ClientsStack.Update(Context.ConnectionId, partyName, true, Dto.Color.BLACK);
            Clients.Group(partyName).addPlayerBlack();
        }

        public void SendMessage(string partyName, string name, string message)
        {
            Clients.Group(partyName).addNewMessageToPage(name, message);
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

        public void AskForReplay(string partyName)
        {
            Clients.Group(partyName).playerAskToReplay();
        }

        public void APlayerAcceptDrawn(string partyName)
        {
            Clients.Group(partyName).playerAcceptDrawn();
        }

        public void APlayerTimeIsExpired(string partyName)
        {
            Clients.Group(partyName).PlayerTimeIsExpired();
        }

        public void RedirectToNewParty(string partyName, string newPartyName)
        {
            Clients.Group(partyName).askToRedirectToNewParty(newPartyName);
        }
    }
}