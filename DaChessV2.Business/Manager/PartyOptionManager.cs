using DaChessV2.Dto;
using System;
using System.Linq;

namespace DaChessV2.Business
{
    public class PartyOptionManager
    {
        public PartyOptionModel GetOptionModel()
        {
            PartyOptionModel toReturn = new PartyOptionModel();

            try
            {
                using (var context = new ChessEntities())
                {
                    foreach(var pc in context.PartyCadence.ToList())
                    {
                        toReturn.CadencesTypes.Add(pc.Id, pc.Wording);
                    }
                }
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = ex.Message;
                toReturn.ErrorDetails = ex.ToString();
            }
            catch (Exception ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMsg = "Erreur non gérée dans la récupération des cadences possibles";
                toReturn.ErrorDetails = ex.ToString();
            }

            return toReturn;
        }
    }
}
