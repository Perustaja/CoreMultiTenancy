using Microsoft.AspNetCore.Mvc;

namespace CoreMultiTenancy.Identity.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}