using System.Linq;

namespace DaChessV2.Business
{
    public static class BoardHelper
    {
        internal static BoardType GetClassic(ChessEntities context)
        {
            return context.BoardType.Where(b => b.Id.Equals(1)).FirstOrDefault();
        }
    }
}
