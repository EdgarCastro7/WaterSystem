using Microsoft.AspNetCore.Mvc;
using WaterSystem.Data;
using WaterSystem.Data.Entities;

namespace WaterSystem.Controllers
{
    public class ConsumptionController : Controller
    {
        private readonly IConsumptionRepository _consumptionRepository;
        private readonly IMeterRepository _meterRepository;

        public ConsumptionController(
            IConsumptionRepository consumptionRepository,
            IMeterRepository meterRepository)
        {
            _consumptionRepository = consumptionRepository;
            _meterRepository = meterRepository;
        }

        public async Task<IActionResult> Index()
        {
            var consumptions = await _consumptionRepository.GetConsumptionsAsync();

            return View(consumptions);
        }
    }
}
