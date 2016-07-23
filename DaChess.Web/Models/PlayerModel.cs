using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaChess.Web.Models
{
    public class PlayerModel : AbstractModel
    {
        public bool IsWhite { get; set; }
        public bool IsBlack { get; set; }
        public string PartyName { get; set; }
    }
}