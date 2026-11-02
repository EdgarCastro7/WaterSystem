using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSystem.Data;
using WaterSystem.Data.Entities;

namespace WaterSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PriceListController : Controller
    {
        private readonly IPriceListRepository _priceListRepository;

        public PriceListController(IPriceListRepository priceListRepository)
        {
            _priceListRepository = priceListRepository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PriceList priceList)
        {
            if (ModelState.IsValid)
            {
                await _priceListRepository.CreateAsync(priceList);

                return RedirectToAction(nameof(Index));
            }

            return View(priceList);
        }

        public async Task<IActionResult> Index()
        {
            var priceLists = await _priceListRepository.GetPriceListsAsync();

            return View(priceLists);
        }
    }
}
