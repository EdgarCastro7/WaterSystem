
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;
using System.IO;
using WaterSystem.Mobile.Services;

namespace WaterSystem.Mobile
{
    public partial class InvoicesPage : ContentPage
    {
        private readonly InvoiceService _invoiceService;
        private readonly UserInfo _user;

        private const string ApiBaseUrl = "https://10.0.2.2:7276/";

        public InvoicesPage(UserInfo user)
        {
            InitializeComponent();

            _invoiceService = new InvoiceService();
            _user = user;

            UserAvatarInitialLabel.Text =
                !string.IsNullOrWhiteSpace(user.FirstName)
                    ? user.FirstName[..1].ToUpperInvariant()
                    : "U";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadUserAvatarAsync();
            await LoadInvoicesAsync();
        }

        private async Task LoadInvoicesAsync()
        {
            InvoicesList.Children.Clear();

            var invoices = await _invoiceService.GetInvoicesAsync();

            if (invoices.Count == 0)
            {
                InvoicesList.Children.Add(new Label
                {
                    Text = "No invoices found.",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#718096")
                });

                return;
            }

            foreach (var invoice in invoices.OrderByDescending(i => i.Date))
            {
                AddInvoiceItem(invoice);
            }
        }

        private void AddInvoiceItem(InvoiceResponse invoice)
        {
            Color statusColor;

            if (invoice.Status == "Approved" && invoice.Payment)
            {
                statusColor = Color.FromArgb("#198754");
            }
            else if (invoice.Status == "Approved" && !invoice.Payment)
            {
                statusColor = Color.FromArgb("#D99A00");
            }
            else
            {
                statusColor = Color.FromArgb("#DC3545");
            }

            string paymentStatus = invoice.Payment ? "Paid" : "Unpaid";

            Border card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E1EAF0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
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
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnSpacing = 10,
                RowSpacing = 7
            };

            Label invoiceLabel = new Label
            {
                Text = $"Invoice #{invoice.Id}",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#123B59")
            };

            Label dateLabel = new Label
            {
                Text = invoice.Date.ToString("dd/MM/yyyy"),
                FontSize = 12,
                TextColor = Color.FromArgb("#718096")
            };

            Label priceLabel = new Label
            {
                Text = $"{invoice.Price.ToString("F2", CultureInfo.GetCultureInfo("pt-PT"))} €",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1769AA"),
                HorizontalTextAlignment = TextAlignment.End
            };

            Label statusLabel = new Label
            {
                Text = invoice.Status == "Approved" ? paymentStatus : invoice.Status,
                FontSize = 12,
                TextColor = statusColor,
                HorizontalTextAlignment = TextAlignment.End
            };

            Label meterLabel = new Label
            {
                Text = $"Meter: #{invoice.MeterId}",
                FontSize = 11,
                TextColor = Color.FromArgb("#718096")
            };

            layout.Add(invoiceLabel, 0, 0);
            layout.Add(dateLabel, 0, 1);
            layout.Add(meterLabel, 0, 2);
            layout.Add(priceLabel, 1, 0);
            layout.Add(statusLabel, 1, 1);

            card.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await Navigation.PushAsync(
                        new InvoiceDetailsPage(invoice.Id));
                })
            });

            card.Content = layout;
            InvoicesList.Children.Add(card);
        }

        private async Task LoadUserAvatarAsync()
        {
            var initial = !string.IsNullOrWhiteSpace(_user.FirstName) ? _user.FirstName[..1].ToUpperInvariant() : "U";

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

        private async void Home_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new DashboardPage(_user));
        }

        private async void Usage_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new UsagePage(_user));
        }

        private async void Profile_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage(_user));
        }
    }
}
