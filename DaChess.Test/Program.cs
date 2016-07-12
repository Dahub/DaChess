using DaChess.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace DaChess.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            IPartyManager manager = DaChess.Business.Factory.Instance.GetPartyManager();
            var tt = manager.New();
            tt = manager.AddPlayerToParty(tt.Id, Colors.BLACK);
            tt = manager.AddPlayerToParty(tt.Id, Colors.WHITE);
            tt.WhiteTurn = false;
            tt = manager.Update(tt);
            
            var hh = tt;
            Console.Read();
        }
    }
}
