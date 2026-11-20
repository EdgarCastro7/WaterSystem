using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WaterSystem.Data;
using WaterSystem.Data.Entities;

namespace WaterSystem.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Client")]
    public class DashboardApiController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMeterRepository _meterRepository;
        private readonly IConsumptionRepository _consumptionRepository;

        private readonly IInvoiceRepository _invoiceRepository;

        public DashboardApiController(
            UserManager<User> userManager,
            IMeterRepository meterRepository,
            IConsumptionRepository consumptionRepository,
            IInvoiceRepository invoiceRepository)
        {
            _userManager = userManager;
            _meterRepository = meterRepository;
            _consumptionRepository = consumptionRepository;
            _invoiceRepository = invoiceRepository;
        }


        [HttpGet("current-consumption")]
        public async Task<IActionResult> GetCurrentConsumption()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var meters = await _meterRepository.GetMeterAsync(user);

            var meterData = new List<(Meter Meter, List<Consumption> Readings)>();

            foreach (var meter in meters)
            {
                var consumptions = await _consumptionRepository.GetConsumptionsAsync(meter);

                var approvedReadings = consumptions.Where(c => c.Status == "Approved").OrderByDescending(c => c.Date)
                    .ThenByDescending(c => c.Id).ToList();

                if (approvedReadings.Count > 0)
                {
                    meterData.Add((meter, approvedReadings));
                }
            }

            if (meterData.Count == 0)
            {
                return NotFound(new
                {
                    message = "No approved readings found."
                });
            }

            var selectedMeter = meterData.OrderByDescending(m => m.Readings[0].Date)
                .ThenByDescending(m => m.Readings[0].Id).First();

            if (selectedMeter.Readings.Count < 2)
            {
                return Ok(new
                {
                    meterId = selectedMeter.Meter.Id,
                    isActive = selectedMeter.Meter.IsActive,
                    consumption = 0,
                    unit = "m³",
                    message = "Not enough approved readings."
                });
            }

            var current = selectedMeter.Readings[0];
            var previous = selectedMeter.Readings[1];

            return Ok(new
            {
                meterId = selectedMeter.Meter.Id,
                consumption = current.Volume - previous.Volume,
                isActive = selectedMeter.Meter.IsActive,
                unit = "m³",
                currentReadingDate = current.Date,
                previousReadingDate = previous.Date
            });
        }


        [HttpGet("last-invoice")]
        public async Task<IActionResult> GetLastInvoice()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var invoice = await _invoiceRepository.GetLastInvoiceAsync(user);

            if (invoice == null)
            {
                return NotFound(new
                {
                    message = "No invoices found."
                });
            }

            return Ok(new
            {
                id = invoice.Id,
                price = invoice.Price,
                payment = invoice.Payment,
                status = invoice.Status,
                date = invoice.DateTime
            });
        }


        [HttpGet("consumption-history")]
        public async Task<IActionResult> GetConsumptionHistory()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var meters = await _meterRepository.GetMeterAsync(user);

            var history = new List<object>();

            foreach (var meter in meters)
            {
                var consumptions = await _consumptionRepository.GetConsumptionsAsync(meter);

                var readings = consumptions.OrderByDescending(c => c.Date).ThenByDescending(c => c.Id)
                    .Select(c => new
                    {
                        id = c.Id,
                        meterId = meter.Id,
                        date = c.Date,
                        volume = c.Volume,
                        status = c.Status,
                        isMeterActive = meter.IsActive
                    })
                    .ToList();

                history.AddRange(readings);
            }

            var orderedHistory = history.OrderByDescending(c => ((dynamic)c).date).ToList();

            return Ok(orderedHistory);
        }
    }
}
