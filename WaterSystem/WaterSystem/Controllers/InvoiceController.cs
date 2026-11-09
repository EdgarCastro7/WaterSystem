using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSystem.Data;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;
using WaterSystem.Models;

namespace WaterSystem.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConsumptionRepository _consumptionRepository;
        private readonly IPriceListRepository _priceListRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly DataContext _context;

        public InvoiceController(IInvoiceRepository invoiceRepository, 
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConsumptionRepository consumptionRepository,
            IPriceListRepository priceListRepository,
            IMeterRepository meterRepository,
            DataContext context)
        {
            _invoiceRepository = invoiceRepository;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _consumptionRepository = consumptionRepository;
            _priceListRepository = priceListRepository;
            _meterRepository = meterRepository;
            _context = context;
        }

        public async Task<IActionResult> Create(int id)
        {
            return View();
        }

        [Authorize(Roles = "Admin,Employee,Client")]
        public async Task<IActionResult> Index()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return Challenge();
            }

            IEnumerable<Invoice> invoices;

            if (User.IsInRole("Client"))
            {
                invoices = await _invoiceRepository.GetInvoicesByUserAsync(user);

                invoices = invoices.Where(i => i.Status == "Approved" || i.Status == "Rejected");
            }
            else
            {
                invoices = await _invoiceRepository.GetInvoicesAsync();
            }

            return View(invoices);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Status != "Pending")
            {
                return BadRequest();
            }

            var user = await _userHelper.GetUserByIdAsync(invoice.UserId);

            if (user == null)
            {
                return NotFound();
            }

            invoice.Status = "Approved";

            await _invoiceRepository.UpdateAsync(invoice);

            var subject = "WaterSystem - New Invoice Available";

            var body = $@"
        <h2>Hello {user.FirstName},</h2>
        <p>Your new water invoice is now available.</p>
        <p><strong>Invoice value:</strong> {invoice.Price:C}</p>
        <p><strong>Invoice date:</strong> {invoice.DateTime:dd/MM/yyyy}</p>
        <p>Please log in to WaterSystem to consult and pay your invoice.</p>
        <p>WaterSystem Team</p>";

            _mailHelper.SendEmail(user.Email, subject, body);

            TempData["ToastMessage"] = "Invoice approved successfully.";
            TempData["ToastType"] = "Success";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Status != "Pending")
            {
                return BadRequest();
            }

            invoice.Status = "Rejected";

            await _invoiceRepository.UpdateAsync(invoice);

            TempData["ToastMessage"] = "Invoice rejected successfully.";
            TempData["ToastType"] = "Warning";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Correct(int id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Status != "Rejected")
            {
                return BadRequest();
            }

            var model = new CorrectInvoiceViewModel
            {
                OriginalInvoiceId = invoice.Id
            };

            return View(model);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Correct(CorrectInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var originalInvoice = await _invoiceRepository.GetByIdAsync(model.OriginalInvoiceId);

            if (originalInvoice == null)
            {
                return NotFound();
            }

            if (originalInvoice.Status != "Rejected")
            {
                return BadRequest();
            }

            var invoices = await _invoiceRepository.GetInvoicesAsync();

            var alreadyCorrected = invoices.Any(i =>i.OriginalInvoiceId == originalInvoice.Id);

            if (alreadyCorrected)
            {
                return BadRequest("This invoice has already been corrected.");
            }

            var correctedInvoice = new Invoice
            {
                DateTime = DateTime.Now,
                Price = model.NewPrice!.Value,
                Payment = false,
                Status = "Pending",
                UserId = originalInvoice.UserId,
                MeterId = originalInvoice.MeterId,
                ConsumptionId = originalInvoice.ConsumptionId,
                OriginalInvoiceId = originalInvoice.Id
            };

            await _invoiceRepository.CreateAsync(correctedInvoice);

            TempData["ToastMessage"] = "Invoice updated successfully.";
            TempData["ToastType"] = "Success";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            if (user == null)
            {
                return Challenge();
            }

            if (invoice.UserId != user.Id)
            {
                return Forbid();
            }

            if (invoice.Status != "Approved" || invoice.Payment)
            {
                return BadRequest();
            }

            invoice.Payment = true;

            await _invoiceRepository.UpdateAsync(invoice);

            TempData["ToastMessage"] = "Invoice paid successfully.";
            TempData["ToastType"] = "Success";

            return RedirectToAction(nameof(Index));
        }
    }
}
