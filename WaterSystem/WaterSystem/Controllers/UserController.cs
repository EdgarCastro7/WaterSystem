using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSystem.Data;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;
using WaterSystem.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WaterSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IGenericRepository<MeterRequest> _meterRequestRepository;

        public UserController(IUserHelper userHelper,
            IMailHelper mailHelper,
            IGenericRepository<MeterRequest> meterRequestRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _meterRequestRepository = meterRequestRepository;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userHelper.GetUsersAsync();

            return View(users);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "Client", "Employee" };

            ViewBag.FromRequest = false;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterNewUserViewModel model,
            string role,
            int? meterRequestId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "Client", "Employee" };
                return View(model);
            }

            if (role != "Client" && role != "Employee")
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
                ViewBag.Roles = new List<string> { "Client", "Employee" };
                return View(model);
            }

            var existingUser = await _userHelper.GetUserByEmailAsync(model.Username);

            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "This email is already registered.");
                ViewBag.Roles = new List<string> { "Client", "Employee" };
                return View(model);
            }

            var user = new User
            {
                UserName = model.Username,
                Email = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                IsActive = true,
            };

            var temporaryPassword = Guid.NewGuid().ToString("N") + "aA1!";
            var result = await _userHelper.AddUserAsync(user, temporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                ViewBag.Roles = new List<string> { "Client", "Employee" };
                return View(model);
            }

            var roleResult = await _userHelper.AddUserToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                var deleteResult = await _userHelper.DeleteUserAsync(user);

                if (!deleteResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "The user was created but the role assignment and cleanup failed.");
                }

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                ViewBag.Roles = new List<string> { "Client", "Employee" };
                return View(model);
            }

            var token = await _userHelper.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new
                {
                    userId = user.Id,
                    token = token
                },
                protocol: Request.Scheme);


            var response = _mailHelper.SendEmail(
    user.Email!,
    "Define your WaterSystem password",
    $"<h2>Welcome to WaterSystem</h2>" +
    $"<p>Click the link below to define your password:</p>" +
    $"<a href=\"{resetLink}\">Define password</a>");

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The user was created, but the password setup email could not be sent.");

                ViewBag.Roles = new List<string> { "Client", "Employee" };
                return View(model);
            }

            if (meterRequestId.HasValue)
            {
                var meterRequest = await _meterRequestRepository.GetByIdAsync(meterRequestId.Value);

                if (meterRequest != null &&
                    meterRequest.Status == "ForwardedToAdmin")
                {
                    meterRequest.Status = "AccountCreated";
                    await _meterRequestRepository.UpdateAsync(meterRequest);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            var existingUser = await _userHelper.GetUserByEmailAsync(model.Username);

            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "This email is already registered.");

                return View(model);
            }

            user.UserName = model.Username;
            user.Email = model.Username;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;

            var result = await _userHelper.UpdateUserAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (currentUser != null && user.Id == currentUser.Id)
            {
                ModelState.AddModelError(string.Empty, "You cannot deactivate your own account.");

                return RedirectToAction(nameof(Index));
            }

            if (user.IsActive && await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                var admins = await _userHelper.GetUsersByRoleAsync("Admin");
                var activeAdmins = admins.Count(a => a.IsActive);

                if (activeAdmins <= 1)
                {
                    ModelState.AddModelError(string.Empty, "The last active administrator cannot be deactivated.");

                    return RedirectToAction(nameof(Index));
                }
            }

            user.IsActive = !user.IsActive;

            var result = await _userHelper.UpdateUserAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        
        public async Task<IActionResult> Delete(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if(user == null)
            {
                return NotFound();
            }

            var result = await _userHelper.DeleteUserAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(user);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CreateFromRequest(int id)
        {
            var request = await _meterRequestRepository.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != "ForwardedToAdmin")
            {
                return BadRequest();
            }

            var model = new RegisterNewUserViewModel
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Email,
                Address = request.Address
            };

            ViewBag.FromRequest = true;
            ViewBag.MeterRequestId = id;

            return View("Create", model);
        }
    }
}
