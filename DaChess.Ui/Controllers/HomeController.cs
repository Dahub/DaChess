using System.Web.Mvc;

namespace DaChess.Ui.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Create", "Party");
        }
    }
}