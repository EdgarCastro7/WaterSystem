using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;

namespace WaterSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Client")]
    public class ProfileApiController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IImageHelper _imageHelper;

        public ProfileApiController(
            UserManager<User> userManager,
            IImageHelper imageHelper)
        {
            _userManager = userManager;
            _imageHelper = imageHelper;
        }

        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(new { message = "No image provided." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            try
            {
                user.ImageUrl = await _imageHelper.UploadImageAsync(image, "users");

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Unable to update profile photo." });
                }

                return Ok(new
                {
                    imageUrl = user.ImageUrl,
                    message = "Profile photo updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("details")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid profile information." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Address = request.Address;
            user.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Unable to update profile.",
                    errors = result.Errors.Select(error => error.Description)
                });
            }

            return Ok(new
            {
                message = "Profile updated successfully.",
                firstName = user.FirstName,
                lastName = user.LastName,
                address = user.Address,
                phoneNumber = user.PhoneNumber,
                fullName = user.FullName
            });
        }
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
