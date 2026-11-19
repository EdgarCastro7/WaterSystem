using System.Globalization;
using System.Net.Http.Headers;
using WaterSystem.Mobile.Services;

namespace WaterSystem.Mobile;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardService _dashboardService;

    private readonly UserInfo _user;

    private const string ApiBaseUrl = "https://10.0.2.2:7276/";

    public DashboardPage(UserInfo user)
    {
        InitializeComponent();

        _dashboardService = new DashboardService();

        _user = user;

        WelcomeLabel.Text = $"Hello, {user.FirstName}!";

        UserAvatarInitialLabel.Text =! string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName[..1].ToUpperInvariant() : "U";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadUserAvatarAsync();

        await LoadDashboardAsync();
        await LoadLastInvoiceAsync();

        WelcomeLabel.Text = $"Hello, {_user.FirstName}!";
    }

    private async Task LoadUserAvatarAsync()
    {
        var initial = !string.IsNullOrWhiteSpace(_user.FirstName)
            ? _user.FirstName[..1].ToUpperInvariant()
            : "U";

        UserAvatarInitialLabel.Text = initial;

        if (string.IsNullOrWhiteSpace(_user.ImageUrl))
        {
            UserAvatarImage.IsVisible = false;
            UserAvatarInitialLabel.IsVisible = true;
            return;
        }

        try
        {
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (message, certificate, chain, errors) => true
            };

            using var client = new HttpClient(handler);
#else
            using var client = new HttpClient();
#endif

            var imageUrl = new Uri(
                new Uri(ApiBaseUrl),
                _user.ImageUrl);

            var imageBytes = await client.GetByteArrayAsync(imageUrl);

            UserAvatarImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

            UserAvatarImage.IsVisible = true;
            UserAvatarInitialLabel.IsVisible = false;
        }
        catch
        {
            UserAvatarImage.IsVisible = false;
            UserAvatarInitialLabel.IsVisible = true;
        }
    }

    private async Task LoadDashboardAsync()
    {
        ConsumptionLabel.Text = "--";
        ConsumptionPeriodLabel.Text = "Loading consumption...";
        MeterLabel.Text = "Meter #--";
        MeterStatusLabel.Text = "Loading...";
        MeterStatusLabel.TextColor = Color.FromArgb("#718096");

        var result = await _dashboardService.GetCurrentConsumptionAsync();

        if (result == null)
        {
            ConsumptionPeriodLabel.Text = "Unable to load consumption.";
            MeterStatusLabel.Text = "Unavailable";
            return;
        }

        if (result.PreviousReadingDate.HasValue && result.CurrentReadingDate.HasValue)
        {
            ConsumptionLabel.Text = result.Consumption.ToString("0.##", CultureInfo.InvariantCulture);
        }
        else
        {
            ConsumptionLabel.Text = "--";
        }

        MeterLabel.Text = $"Meter #{result.MeterId}";

        MeterStatusLabel.Text = result.IsActive ? "Active" : "Inactive";

        MeterStatusLabel.TextColor = result.IsActive
            ? Color.FromArgb("#15803D")
            : Color.FromArgb("#B91C1C");

        if (result.PreviousReadingDate.HasValue &&
            result.CurrentReadingDate.HasValue)
        {
            var previousDate = result.PreviousReadingDate.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

            var currentDate = result.CurrentReadingDate.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

            ConsumptionPeriodLabel.Text = $"{previousDate} - {currentDate}";
        }
        else
        {
            ConsumptionPeriodLabel.Text = result.Message ?? "Not enough approved readings.";
        }
    }

    private async Task LoadLastInvoiceAsync()
    {
        InvoicePriceLabel.Text = "--";
        InvoiceStatusLabel.Text = "Loading...";
        InvoiceStatusLabel.TextColor = Color.FromArgb("#718096");

        var invoice = await _dashboardService.GetLastInvoiceAsync();

        if (invoice == null)
        {
            InvoicePriceLabel.Text = "--";
            InvoiceStatusLabel.Text = "No invoices yet";
            InvoiceStatusLabel.TextColor = Color.FromArgb("#718096");
            return;
        }

        InvoicePriceLabel.Text = $"€{invoice.Price.ToString("0.00", CultureInfo.InvariantCulture)}";

        if (invoice.Payment)
        {
            InvoiceStatusLabel.Text = "Paid";
            InvoiceStatusLabel.TextColor = Color.FromArgb("#15803D");
        }
        else
        {
            InvoiceStatusLabel.Text = "Unpaid";
            InvoiceStatusLabel.TextColor = Color.FromArgb("#B91C1C");
        }
    }

    private async void Profile_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage(_user));
    }
}
