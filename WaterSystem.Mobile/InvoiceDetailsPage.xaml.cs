
using System.Globalization;
using WaterSystem.Mobile.Services;

namespace WaterSystem.Mobile
{
    public partial class InvoiceDetailsPage : ContentPage
    {
        private readonly InvoiceService _invoiceService;
        private readonly int _invoiceId;

        public InvoiceDetailsPage(int invoiceId)
        {
            InitializeComponent();

            _invoiceService = new InvoiceService();
            _invoiceId = invoiceId;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadInvoiceDetailsAsync();
        }

        private async Task LoadInvoiceDetailsAsync()
        {
            var invoice = await _invoiceService.GetInvoiceDetailsAsync(_invoiceId);

            if (invoice == null)
            {
                await DisplayAlert(
                    "Error",
                    "Unable to load invoice details.",
                    "OK");

                await Navigation.PopAsync();
                return;
            }

            InvoiceNumberLabel.Text = $"Invoice #{invoice.Id}";

            InvoiceDateLabel.Text = invoice.Date.ToString("dd/MM/yyyy");

            MeterLabel.Text = $"#{invoice.MeterId}";

            ConsumptionIdLabel.Text = $"#{invoice.ConsumptionId}";

            ConsumptionDateLabel.Text =
                invoice.ConsumptionDate.ToString("dd/MM/yyyy");

            ConsumptionVolumeLabel.Text =
                $"{invoice.ConsumptionVolume.ToString("F1", CultureInfo.GetCultureInfo("pt-PT"))} m³";

            PriceLabel.Text =
                $"{invoice.Price.ToString("F2", CultureInfo.GetCultureInfo("pt-PT"))} €";

            if (invoice.Status == "Approved" && invoice.Payment)
            {
                PaymentStatusLabel.Text = "Paid";
                PaymentStatusLabel.TextColor = Color.FromArgb("#198754");
            }
            else if (invoice.Status == "Approved" && !invoice.Payment)
            {
                PaymentStatusLabel.Text = "Unpaid";
                PaymentStatusLabel.TextColor = Color.FromArgb("#D99A00");
            }
            else
            {
                PaymentStatusLabel.Text = invoice.Status;
                PaymentStatusLabel.TextColor = Color.FromArgb("#DC3545");
            }
        }

        private async void Back_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
