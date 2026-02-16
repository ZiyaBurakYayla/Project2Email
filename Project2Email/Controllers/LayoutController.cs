using Microsoft.AspNetCore.Mvc;

namespace Project2Email.Controllers
{
    public class LayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
