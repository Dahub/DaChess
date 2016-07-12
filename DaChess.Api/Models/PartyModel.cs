using System;

namespace DaChess.Api.Models
{
    public class PartyModel
    {
        public string ErrorMessage { get; set; }
        public bool IsError { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Board { get; set; }
        public string WhiteToken { get; set; }
        public string BlackToken { get; set; }
        public bool WhiteTurn { get; set; }
        public bool WhiteAskToPlay { get; set; }
        public bool BlackAskToPlay { get; set; }
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