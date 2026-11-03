using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSystem.Data;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;

namespace WaterSystem.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserHelper _userHelper;

        public InvoiceController(IInvoiceRepository invoiceRepository, IUserHelper userHelper)
        {
            _invoiceRepository = invoiceRepository;
            _userHelper = userHelper;
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

            invoice.Status = "Approved";

            await _invoiceRepository.UpdateAsync(invoice);

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

            return RedirectToAction(nameof(Index));
        }
    }
}
