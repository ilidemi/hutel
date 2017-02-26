using Microsoft.AspNetCore.Mvc;

namespace hutel.Controllers
{
    public class UiController : Controller
    {
        [HttpGet("")]
        public ActionResult Index()
        {
            return View();
        }
    }
}