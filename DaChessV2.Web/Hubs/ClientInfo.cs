using DaChessV2.Dto;

namespace DaChessV2.Web.Hubs
{
    public class ClientInfo
    {
        public string PartyName { get; set; }
        public bool IsPlayer { get; set; }
        public Color? Color { get; set; }        
    }
}