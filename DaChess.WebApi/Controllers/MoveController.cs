using DaChess.Business;
using DaChess.WebApi.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace DaChess.WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MoveController : ApiController
    {
        public JsonResult<MoveModel> Get(HttpRequestMessage request)
        {
            string partyName = request.GetQueryNameValuePairs().Where(k => k.Key.ToLower().Equals("name")).First().Value;
            string move = request.GetQueryNameValuePairs().Where(k => k.Key.ToLower().Equals("move")).First().Value;
            string token = request.GetQueryNameValuePairs().Where(k => k.Key.ToLower().Equals("token")).First().Value;

            MoveModel toReturn = new MoveModel();

            try
            {
                IBoardManager manager = Factory.Instance.GetBoardManager();
                manager.MakeMove(move, partyName, token);
                toReturn.Board = manager.ToJsonString();
            }
            catch (DaChessException ex)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                toReturn.IsError = true;
                toReturn.ErrorMessage = "Erreur non gérée dans la l'application du coup";
            }

            return Json(toReturn);
        }
    }
}
