using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WaterSystem.Data;
using WaterSystem.Helpers;
using WaterSystem.Models;

namespace WaterSystem.Controllers.API
{
    [Authorize(Roles = "Client")]
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
    }
}
