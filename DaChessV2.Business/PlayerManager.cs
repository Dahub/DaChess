using DaChessV2.Dto;
using System;

namespace DaChessV2.Business
{
    public class PlayerManager
    {
        /// <summary>
        /// Retourne un PlayerModel contenant la couleur du joueur à partir de son token et du nom de la partie
        /// </summary>
        /// <param name="token">token du joueur</param>
        /// <param name="partyName">nom de la partie concernée</param>
        /// <returns>le PlayerModel avec la couleur renseignée</returns>
        public PlayerModel GetPlayerColor(string token, string partyName, string currentPartyName)
        {
            PlayerModel toReturn = new PlayerModel();

            try
            {
                if (String.IsNullOrEmpty(token) || String.IsNullOrEmpty(partyName))
                    throw new DaChessException("Token ou nom de partie absent");

                if (partyName != currentPartyName)
                    throw new DaChessException("Pas de cookies pour cette partie");
                    
                Party p = PartyHelper.GetByName(partyName);
                string[] infos = CryptoHelper.Decrypt(token, p.Seed).Split("#;#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int color = (Int32.Parse(infos[1]));

                toReturn.PartyName = partyName;
                toReturn.Token = token;
                toReturn.PlayerColor = (Color)color;
                toReturn.ResultText = "Cookies récupéré";
            }
            catch (Exception ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = "Erreur lors de l'extranction de la couleur du joueur";
                toReturn.ErrorDetails = ex.ToString();          
            }

            return toReturn;
        }
    }
}
