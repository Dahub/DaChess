namespace DaChessV2.Business
{
    internal class LogHelper
    {
        internal static void AddLog(LogLevel logType, string msg, string details)
        {
            try
            {
                using (var context = new ChessEntities())
                {
                    context.Logs.Add(new Logs()
                    {
                        FK_LogType = (int)logType,
                        Details = details,
                        Wording = msg
                    });
                    context.SaveChanges();
                }
            }
            catch { } // catch vide, le log ne fait pas planter l'appli
        }
    }
}
