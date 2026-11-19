using WaterSystem.Mobile.Services;

namespace WaterSystem.Mobile;

public partial class MainPage : ContentPage
{
    private readonly AuthService _authService;

    public MainPage()
    {
        InitializeComponent();
        _authService = new AuthService();
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Login", "Please enter your email and password.", "OK");
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text = "Signing in...";

        var result = await _authService.LoginAsync(email, password);

        LoginButton.IsEnabled = true;
        LoginButton.Text = "Sign In";

        if (result == null)
        {
            await DisplayAlert(
                "Login failed",
                "Invalid credentials or unable to connect to the server.",
                "OK");

            return;
        }

        await Navigation.PushAsync(new DashboardPage(result.User));
    }
}
