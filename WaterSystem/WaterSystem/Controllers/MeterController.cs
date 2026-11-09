using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSystem.Data;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;
using WaterSystem.Models;

namespace WaterSystem.Controllers
{
    public class MeterController : Controller
    {
        private readonly IMeterRepository _meterRepository;
        private readonly IGenericRepository<Meter> _genericRepository;
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;

        public MeterController(IMeterRepository meterRepository,
            IGenericRepository<Meter> genericRepository,
            IUserHelper userHelper,
            DataContext context)
        {
            _meterRepository = meterRepository;
            _genericRepository = genericRepository;
            _userHelper = userHelper;
            _context = context;
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

        [Authorize(Roles = "Client")]
        public IActionResult RequestAdditional()
        {
            return View(new AdditionalMeterRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> RequestAdditional(AdditionalMeterRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null || !user.IsActive)
            {
                return Forbid();
            }

            var request = new MeterRequest
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Address = model.Address,
                RequestDate = DateTime.Today,
                Status = "Pending",
                UserId = user.Id
            };

            await _context.MeterRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CreateFromRequest(int id)
        {
            var request = await _context.MeterRequests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.UserId == null || request.Status != "ForwardedToAdmin")
            {
                return BadRequest();
            }

            var user = await _userHelper.GetUserByIdAsync(request.UserId);

            if (user == null || !user.IsActive ||
                !await _userHelper.IsUserInRoleAsync(user, "Client"))
            {
                return BadRequest();
            }

            var meter = new Meter
            {
                UserId = user.Id,
                InstallationDate = DateTime.Today,
                IsActive = true
            };

            await _genericRepository.CreateAsync(meter);

            request.Status = "MeterCreated";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), "MeterRequest");
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
