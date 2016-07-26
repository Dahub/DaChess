using System;

namespace DaChess.Web.Models
{
    public class PartyModel : AbstractModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Board { get; set; }
        public string History { get; set; }
        public string WhiteToken { get; set; }
        public string BlackToken { get; set; }
        public bool WhiteTurn { get; set; }
        public bool WhiteAskToPlay { get; set; }
        public bool BlackAskToPlay { get; set; }
        public bool WhiteIsCheck { get; set; }
        public bool BlackIsCheck { get; set; }
        public bool WhiteCanPromote { get; set; }
        public bool BlackCanPromote { get; set; }
        public bool WhiteIsCheckMat { get; set; }
        public bool BlackIsCheckMat { get; set; }
        public bool WhiteIsPat { get; set; }
        public bool BlackIsPat { get; set; }
        public bool IsReady
        {
            get
            {
                if (Id == 0)
                    return false;
                if (String.IsNullOrEmpty(Name))
                    return false;
                if (String.IsNullOrEmpty(Board))
                    return false;
                if (String.IsNullOrEmpty(WhiteToken))
                    return false;
                if (String.IsNullOrEmpty(BlackToken))
                    return false;
                return true;
            }
        }
    }
}