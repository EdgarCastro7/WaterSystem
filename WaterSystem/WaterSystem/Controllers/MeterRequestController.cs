using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSystem.Data;
using WaterSystem.Data.Entities;
using WaterSystem.Models;

namespace WaterSystem.Controllers
{
    public class MeterRequestController : Controller
    {
        private readonly IGenericRepository<MeterRequest> _repository;

        public MeterRequestController(IGenericRepository<MeterRequest> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MeterRequestViewModel());
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(MeterRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new MeterRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Address = model.Address,
                RequestDate = DateTime.Today,
                Status = "Pending"
            };

            await _repository.CreateAsync(request);

            return RedirectToAction(nameof(Create));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles ="Employee")]
        public async Task<IActionResult> Approve(int id)
        {
            var approve = await _repository.GetByIdAsync(id);

            if (approve == null)
            {
                return NotFound();
            }

            if(approve.Status != "Pending")
            {
                return BadRequest();
            }

            approve.Status = "Approved";

            await _repository.UpdateAsync(approve);

            return RedirectToAction(nameof(Index));
        }



        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _repository.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != "Pending")
            {
                return BadRequest();
            }

            request.Status = "Rejected";

            await _repository.UpdateAsync(request);

            return RedirectToAction(nameof(Index));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> ForwardToAdmin(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if(request == null)
            {
                return NotFound();
            }

            if(request.Status != "Approved")
            {
                return BadRequest();
            }

            request.Status = "ForwardedToAdmin";

            await _repository.UpdateAsync(request);

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles ="Employee, Admin")]
        public async Task<IActionResult> Index()
        {
            bool isAdmin = User.IsInRole("Admin");

            var allRequest = await _repository.GetAll().ToListAsync();

            if (isAdmin)
            {
                allRequest = allRequest.Where(r => r.Status == "ForwardedToAdmin").ToList();
            }

            return View(allRequest);
        }
    }
}
