using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSystem.Data;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;

namespace WaterSystem.Controllers
{
    public class MeterController : Controller
    {
        private readonly IMeterRepository _meterRepository;
        private readonly IGenericRepository<Meter> _genericRepository;
        private readonly IUserHelper _userHelper;

        public MeterController(IMeterRepository meterRepository,
            IGenericRepository<Meter> genericRepository,
            IUserHelper userHelper)
        {
            _meterRepository = meterRepository;
            _genericRepository = genericRepository;
            _userHelper = userHelper;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var meters = await _genericRepository.GetAll().Include(m => m.User).ToListAsync();
                return View(meters);
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return Forbid();
            }

            var userMeters = await _meterRepository.GetMeterAsync(user);

            return View(userMeters);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var clients = await _userHelper.GetUsersByRoleAsync("Client");

            ViewBag.Clients = clients.Where(c => c.IsActive);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Meter meter)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage);

                var errorMessage = string.Join(" | ", errors);

                return BadRequest($"Invalid model: {errorMessage}");
            }

            var client = await _userHelper.GetUserByIdAsync(meter.UserId);

            if (client == null ||
                !await _userHelper.IsUserInRoleAsync(client, "Client") ||
                !client.IsActive)
            {
                return BadRequest("The selected client is invalid, inactive, or does not have the Client role.");
            }

            meter.IsActive = true;

            await _genericRepository.CreateAsync(meter);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meter = await _genericRepository.GetByIdAsync(id.Value);

            if (meter == null)
            {
                return NotFound();
            }

            return View(meter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Meter meter)
        {
            if (id != meter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingMeter = await _genericRepository.GetByIdAsync(id);

                if (existingMeter == null)
                {
                    return NotFound();
                }

                existingMeter.InstallationDate = meter.InstallationDate;
                existingMeter.IsActive = meter.IsActive;

                await _genericRepository.UpdateAsync(existingMeter);

                return RedirectToAction(nameof(Index));
            }

            return View(meter);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meter = await _genericRepository.GetByIdAsync(id.Value);

            if (meter == null)
            {
                return NotFound();
            }

            return View(meter);
        }
    }
}
