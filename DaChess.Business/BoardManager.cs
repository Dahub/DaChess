using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DaChess.Business
{
    internal class BoardManager : IBoardManager
    {
        internal IList<BoardCase> BoardCases { get; set; }

        internal BoardManager() { BoardCases = new List<BoardCase>(); }

        public void Init(string jsonBoard)
        {
            BoardCases = new List<BoardCase>();

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(jsonBoard);
            object[] cases = (object[])(routes_list["board"]);
            for (int i = 0; i < cases.Length; i++)
            {
                BoardCases.Add(new BoardCase((IDictionary<string, object>)(cases[i])));
            }
        }

        public string ToJsonString()
        {
            return @"{
		        ""board"":[{0}]}";
        }
    }

    internal class BoardCase
    {
        internal BoardCase(IDictionary<string, object> values)
        {
            if (values.ContainsKey("col"))
            {
                this.Col = values["col"].ToString();
            }
            if (values.ContainsKey("line"))
            {
                this.Line = values["line"].ToString();
            }
            if (values.ContainsKey("piece"))
            {
                this.Piece = values["piece"].ToString();
            }
        }

        public string Col { get; set; }
        public string Line { get; set; }
        public string Piece { get; set; }

        public string ToJsonString()
        {
            return string.Format(@"{
                ""col"" :""{0}"",
				""line"" : ""{1}"",
				""piece"" : ""{2}""
            }", this.Col, this.Line, this.Piece);
        }
    }
}
