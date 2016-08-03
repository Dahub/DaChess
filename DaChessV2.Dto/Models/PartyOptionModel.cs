using System.Collections.Generic;

namespace DaChessV2.Dto
{
    public class PartyOptionModel : AbstractModel
    {
        public PartyOptionModel()
        {
            CadencesTypes = new Dictionary<int, string>();
        }

        public EnumPartyCadence ChoisedCadenceType { get; set; }
        public int TimeInSeconds { get; set; }
        public int FischerInSeconds { get; set; }
        public IDictionary<int, string> CadencesTypes { get; set; } 
    }
}