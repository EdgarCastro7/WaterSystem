using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WaterSystem.Data;
using WaterSystem.Helpers;
using WaterSystem.Models;

namespace WaterSystem.Controllers.API
{
    [Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Client")]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceApiController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserHelper _userHelper;

        public InvoiceApiController(IInvoiceRepository invoiceRepository,
            IUserHelper userHelper)
        {
            _invoiceRepository = invoiceRepository;
            _userHelper = userHelper;
        }

        [HttpGet("last-to-pay")]
        public async Task<IActionResult> GetLastInvoiceToPay()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return Unauthorized();
            }

            var invoice = await _invoiceRepository.GetLastInvoiceToPayAsync(user);

            if (invoice == null)
            {
                return NotFound("No unpaid invoices found.");
            }

            var model = new InvoiceApiViewModel
            {
                InvoiceId = invoice.Id,
                DateTime = invoice.DateTime,
                Price = invoice.Price,
                Status = invoice.Status,
                Payment = invoice.Payment,
                MeterId = invoice.MeterId,
                ConsumptionId = invoice.ConsumptionId
            };

            return Ok(model);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceDetails(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized();
            }

            var invoice = await _invoiceRepository.GetInvoiceDetailsAsync(id);

            if (invoice == null || invoice.UserId != user.Id)
            {
                return NotFound();
            }

            var result = new
            {
                id = invoice.Id,
                date = invoice.DateTime,
                price = invoice.Price,
                payment = invoice.Payment,
                status = invoice.Status,
                meterId = invoice.MeterId,
                consumptionId = invoice.ConsumptionId,
                consumptionDate = invoice.Consumption.Date,
                consumptionVolume = invoice.Consumption.Volume
            };

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized();
            }

            var invoices = await _invoiceRepository.GetInvoicesByUserAsync(user);

            var result = invoices
                .OrderByDescending(i => i.DateTime)
                .Select(i => new
                {
                    id = i.Id,
                    date = i.DateTime,
                    price = i.Price,
                    payment = i.Payment,
                    status = i.Status,
                    meterId = i.MeterId,
                })
                .ToList();

            return Ok(result);
        }
    }
}
