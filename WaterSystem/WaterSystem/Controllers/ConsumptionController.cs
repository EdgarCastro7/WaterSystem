using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.Metrics;
using System.Threading;
using WaterSystem.Data;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;
using WaterSystem.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WaterSystem.Controllers
{
    public class ConsumptionController : Controller
    {
        private readonly IConsumptionRepository _consumptionRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IUserHelper _userHelper;
        private readonly IInvoiceRepository _invoiceRepository;

        public ConsumptionController(
            IConsumptionRepository consumptionRepository,
            IMeterRepository meterRepository,
            IUserHelper userHelper,
            IInvoiceRepository invoiceRepository)
        {
            _consumptionRepository = consumptionRepository;
            _meterRepository = meterRepository;
            _userHelper = userHelper;
            _invoiceRepository = invoiceRepository;
        }


        [Authorize(Roles = "Employee, Client")]
        public async Task<IActionResult> Index()
        {
            var consumptions = await _consumptionRepository.GetConsumptionsAsync();

            if (User.IsInRole("Client"))
            {
                var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (client == null)
                {
                    return NotFound("Client not found.");
                }

                consumptions = consumptions
                    .Where(c => c.Meter != null && c.Meter.UserId == client.Id)
                    .ToList();
            }

            return View(consumptions);
        }


        [Authorize(Roles = "Employee, Client")]
        public async Task<IActionResult> Create()
        {
            var meters = await _meterRepository.GetAll().Where(m => m.IsActive).ToListAsync();

            if (User.IsInRole("Client"))
            {
                var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (client == null)
                {
                    return NotFound("Client not found");
                }

                meters = meters.Where(m => m.UserId == client.Id).ToList();
            }

            ViewBag.Meters = meters;

            return View(new ConsumptionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee, Client")]
        public async Task<IActionResult> Create(ConsumptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var meter = await _meterRepository.GetByIdAsync(model.MeterId);

                if (meter == null)
                {
                    return NotFound();
                }

                if (User.IsInRole("Client"))
                {
                    var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                    if (client == null)
                    {
                        return NotFound("Client not found");
                    }

                    if (client.Id != meter.UserId)
                    {
                        return Forbid();
                    }
                }

                if (!meter.IsActive)
                {
                    return BadRequest("The meter is not activated");
                }

                if (model.Volume <= 0)
                {
                    ModelState.AddModelError(nameof(model.Volume),
                        "The volume must be greater than zero.");
                }

                var lastConsumption = await _consumptionRepository
                    .GetLastConsumptionAsync(meter);

                if (lastConsumption != null)
                {
                    if (model.Volume <= lastConsumption.Volume)
                    {
                        ModelState.AddModelError(nameof(model.Volume),
                            "The meter reading must be greater than the previous reading.");
                    }

                    if (model.Date.Date <= lastConsumption.Date.Date)
                    {
                        ModelState.AddModelError(nameof(model.Date),
                            "The reading date must be later than the previous reading.");
                    }
                }

                var exists = await _consumptionRepository
                    .AlreadyExistsAsync(meter, model.Date);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Date), "A reading already exists for this date.");
                }

                if (ModelState.IsValid)
                {
                    var consumption = new Consumption
                    {
                        MeterId = model.MeterId,
                        Date = model.Date,
                        Volume = model.Volume
                    };

                    await _consumptionRepository.CreateAsync(consumption);

                    return RedirectToAction(nameof(Index));
                }
            }

            var activeMeters = await _meterRepository.GetAll().Where(m => m.IsActive).ToListAsync();

            if (User.IsInRole("Client"))
            {
                var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (client == null)
                {
                    return NotFound("Client not found");
                }

                activeMeters = activeMeters.Where(m => m.UserId == client.Id).ToList();
            }

            ViewBag.Meters = activeMeters;

            return View(model);
        }

        [Authorize(Roles = "Employee, Client")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consumption = await _consumptionRepository.GetByIdAsync(id.Value);

            if (consumption == null)
            {
                return NotFound();
            }

            var meter = await _meterRepository.GetByIdAsync(consumption.MeterId);

            if (meter == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client"))
            {
                var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (client == null)
                {
                    return NotFound();
                }

                if (client.Id != meter.UserId)
                {
                    return Forbid();
                }
            }

            var model = new ConsumptionViewModel
            {
                Id = consumption.Id,
                MeterId = consumption.MeterId,
                Date = consumption.Date,
                Volume = consumption.Volume,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee, Client")]
        public async Task<IActionResult> Edit(ConsumptionViewModel model)
        {
            if (model == null)
            {
                return NotFound();
            }

            var consumption = await _consumptionRepository.GetByIdAsync(model.Id);

            if (consumption == null)
            {
                return NotFound();
            }

            var meter = await _meterRepository.GetByIdAsync(consumption.MeterId);

            if (meter == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client"))
            {
                var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (client == null)
                {
                    return NotFound();
                }

                if (client.Id != meter.UserId)
                {
                    return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                if (model.Volume <= 0)
                {
                    ModelState.AddModelError(nameof(model.Volume), "The meter reading must be greater than zero.");
                }

                var consumptions = await _consumptionRepository.GetConsumptionsAsync(meter);

                var otherConsumptions = consumptions
                    .Where(c => c.Id != consumption.Id)
                    .ToList();

                if (otherConsumptions.Any(c => c.Date.Date == model.Date.Date))
                {
                    ModelState.AddModelError(nameof(model.Date), "A reading already exists for this date.");
                }

                var previousById = otherConsumptions
                    .Where(c => c.Id < consumption.Id)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault();

                var nextById = otherConsumptions
                    .Where(c => c.Id > consumption.Id)
                    .OrderBy(c => c.Id)
                    .FirstOrDefault();

                if (previousById != null)
                {
                    if (model.Date.Date < previousById.Date.Date)
                    {
                        ModelState.AddModelError(nameof(model.Date), "The date cannot be earlier than the previous consumption.");
                    }

                    if (model.Volume <= previousById.Volume)
                    {
                        ModelState.AddModelError(nameof(model.Volume), "The meter reading must be greater than the previous reading.");
                    }
                }

                if (nextById != null)
                {
                    if (model.Date.Date > nextById.Date.Date)
                    {
                        ModelState.AddModelError(nameof(model.Date), "The date cannot be later than the next consumption.");
                    }

                    if (model.Volume >= nextById.Volume)
                    {
                        ModelState.AddModelError(nameof(model.Volume), "The meter reading must be lower than the next reading.");
                    }
                }

                if (ModelState.IsValid)
                {
                    consumption.Date = model.Date;
                    consumption.Volume = model.Volume;

                    await _consumptionRepository.UpdateAsync(consumption);

                    return RedirectToAction(nameof(Index));
                }
            }

            model.MeterId = consumption.MeterId;

            return View(model);
        }

        [Authorize(Roles = "Employee, Client")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consumption = await _consumptionRepository.GetByIdAsync(id.Value);

            if (consumption == null)
            {
                return NotFound();
            }

            var meter = await _meterRepository.GetByIdAsync(consumption.MeterId);

            if (meter == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client"))
            {
                var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (client == null)
                {
                    return NotFound();
                }

                if (client.Id != meter.UserId)
                {
                    return Forbid();
                }
            }

            return View(consumption);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee, Client")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consumption = await _consumptionRepository.GetByIdAsync(id);

            if (consumption == null)
            {
                return NotFound();
            }

            var meter = await _meterRepository.GetByIdAsync(consumption.MeterId);

            if (meter == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client"))
            {
                var client = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

                if (client == null)
                {
                    return NotFound();
                }

                if (client.Id != meter.UserId)
                {
                    return Forbid();
                }
            }

            var hasInvoice = await _invoiceRepository.ExistsByConsumptionAsync(consumption.Id);

            if (hasInvoice)
            {
                return BadRequest("This consumption cannot be deleted because it has an associated invoice.");
            }

            await _consumptionRepository.DeleteAsync(consumption);

            return RedirectToAction(nameof(Index));
        }
    }
}
