using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaChessV2.Dto
{
    public class PlayerModel : AbstractModel
    {
        public Color PlayerColor { get; set; }
        public string Token { get; set; }
    }
}
