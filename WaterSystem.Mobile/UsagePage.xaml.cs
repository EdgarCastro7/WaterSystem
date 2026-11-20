
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;
using WaterSystem.Mobile.Services;


namespace WaterSystem.Mobile
{
    public partial class UsagePage : ContentPage
    {
        private readonly DashboardService _dashboardService;
        private readonly UserInfo _user;

        private const string ApiBaseUrl = "https://10.0.2.2:7276/";

        public UsagePage(UserInfo user)
        {
            InitializeComponent();

            _dashboardService = new DashboardService();
            _user = user;

            UserAvatarInitialLabel.Text = !string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName[..1].ToUpperInvariant() : "U";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadUserAvatarAsync();
            await LoadUsageAsync();
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

                UserAvatarImage.Source = ImageSource.FromStream(
                    () => new MemoryStream(imageBytes));

                UserAvatarImage.IsVisible = true;
                UserAvatarInitialLabel.IsVisible = false;
            }
            catch
            {
                UserAvatarImage.IsVisible = false;
                UserAvatarInitialLabel.IsVisible = true;
            }
        }

        private async Task LoadUsageAsync()
        {
            CurrentConsumptionLabel.Text = "--";
            ConsumptionPeriodLabel.Text = "Loading...";
            CurrentMeterLabel.Text = "Meter: --";

            HistoryList.Children.Clear();

            var currentConsumption = await _dashboardService
                .GetCurrentConsumptionAsync();

            if (currentConsumption != null)
            {
                CurrentConsumptionLabel.Text =
                    currentConsumption.Consumption.ToString("F1", CultureInfo.InvariantCulture);

                if (currentConsumption.CurrentReadingDate.HasValue &&
                    currentConsumption.PreviousReadingDate.HasValue)
                {
                    ConsumptionPeriodLabel.Text =
                        $"{currentConsumption.PreviousReadingDate.Value:dd/MM/yyyy} - " +
                        $"{currentConsumption.CurrentReadingDate.Value:dd/MM/yyyy}";
                }
                else
                {
                    ConsumptionPeriodLabel.Text =
                        currentConsumption.Message ?? "No consumption period available.";
                }

                CurrentMeterLabel.Text =
                    $"Meter: #{currentConsumption.MeterId}";
            }
            else
            {
                ConsumptionPeriodLabel.Text = "No consumption data available.";
            }

            var history = await _dashboardService
                .GetConsumptionHistoryAsync();

            if (history.Count == 0)
            {
                AddHistoryMessage("No consumption history found.");
                return;
            }

            foreach (var item in history.OrderByDescending(c => c.Date))
            {
                AddHistoryItem(item);
            }
        }

        private void AddHistoryItem(ConsumptionHistoryResponse item)
        {
            Color statusColor = item.Status == "Approved" ? Color.FromArgb("#198754")
                : item.Status == "Pending"
                    ? Color.FromArgb("#D99A00")
                    : Color.FromArgb("#DC3545");

            Border card = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 12
                },
                Padding = 16
            };

            Grid layout = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnSpacing = 10,
                RowSpacing = 6
            };

            Label dateLabel = new Label
            {
                Text = item.Date.ToString("dd/MM/yyyy"),
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#243447")
            };

            Label meterLabel = new Label
            {
                Text = $"Meter: #{item.MeterId}",
                FontSize = 12,
                TextColor = Color.FromArgb("#718096")
            };

            Label volumeLabel = new Label
            {
                Text = $"{item.Volume:F1} m³",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1769AA"),
                HorizontalTextAlignment = TextAlignment.End
            };

            Label statusLabel = new Label
            {
                Text = item.Status,
                FontSize = 12,
                TextColor = statusColor,
                HorizontalTextAlignment = TextAlignment.End
            };

            layout.Add(dateLabel, 0, 0);
            layout.Add(meterLabel, 0, 1);
            layout.Add(volumeLabel, 1, 0);
            layout.Add(statusLabel, 1, 1);

            card.Content = layout;

            HistoryList.Children.Add(card);
        }

        private void AddHistoryMessage(string message)
        {
            HistoryList.Children.Add(new Label
            {
                Text = message,
                FontSize = 14,
                TextColor = Color.FromArgb("#718096")
            });
        }
    
        private async void Home_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void Profile_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage(_user));
        }
    }
}
