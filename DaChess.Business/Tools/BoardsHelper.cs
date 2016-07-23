using System.Linq;

namespace DaChess.Business
{
    internal static class BoardsHelper
    {
        internal static Board GetClassic(ChessEntities context)
        {
            return context.Boards.Where(b => b.Id.Equals(1)).FirstOrDefault();
        }
    }
}
