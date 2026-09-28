using Microsoft.AspNetCore.Mvc;

namespace WaterSystem.Controllers
{
    public class ConsumptionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
