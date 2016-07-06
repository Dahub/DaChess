using System.Linq;

namespace DaChess.Business
{
    internal static class Boards
    {
        internal static Board Classic(ChessEntities context)
        {
            return context.Boards.Where(b => b.Id.Equals(1)).FirstOrDefault();
        }
    }
}
