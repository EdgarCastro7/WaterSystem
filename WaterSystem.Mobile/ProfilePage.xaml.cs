using System.Net.Http.Headers;
using System.Text.Json;
using WaterSystem.Mobile.Services;

namespace WaterSystem.Mobile;

public partial class ProfilePage : ContentPage
{
    private readonly UserInfo _user;

    private byte[]? _selectedImageBytes;

    private const string ApiBaseUrl = "https://10.0.2.2:7276/";

    public ProfilePage(UserInfo user)
    {
        InitializeComponent();

        _user = user;

        LoadUserInformation();
    }

    private async void LoadUserInformation()
    {
        FirstNameEntry.Text = _user.FirstName;
        LastNameEntry.Text = _user.LastName;
        AddressEntry.Text = _user.Address;
        PhoneEntry.Text = _user.PhoneNumber;

        var initial = !string.IsNullOrWhiteSpace(_user.FirstName) ? _user.FirstName[..1].ToUpperInvariant() : "U";

        AvatarInitialLabel.Text = initial;

        // Load existing profile photo
        if (!string.IsNullOrWhiteSpace(_user.ImageUrl))
        {
            await LoadProfileImageAsync(_user.ImageUrl);
        }
    }

    private async void ChangeProfilePhoto_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await FilePicker.Default.PickAsync(
                new PickOptions
                {
                    PickerTitle = "Choose a profile photo",
                    FileTypes = FilePickerFileType.Images
                });

            if (photo == null)
            {
                return;
            }

            using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);

            _selectedImageBytes = memoryStream.ToArray();

            var imageBytes = _selectedImageBytes;

            // Preview selected photo
            ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

            ProfileImage.IsVisible = true;
            AvatarInitialLabel.IsVisible = false;

            PhotoStatusLabel.Text = "Uploading photo...";

            // Upload photo to API
            await UploadProfilePhotoAsync(photo.FileName);

        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Profile Photo",
                $"Unable to select photo: {ex.Message}",
                "OK");
        }
    }

    private async Task UploadProfilePhotoAsync(string fileName)
    {
        try
        {
            if (_selectedImageBytes == null)
            {
                return;
            }

            var token = await SecureStorage.Default.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlert(
                    "Authentication",
                    "Your session has expired. Please log in again.",
                    "OK");

                return;
            }

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

            client.BaseAddress = new Uri(ApiBaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();

            var imageContent = new ByteArrayContent(_selectedImageBytes);
            imageContent.Headers.ContentType =
                new MediaTypeHeaderValue("image/jpeg");

            content.Add(imageContent, "image", fileName);

            var response = await client.PostAsync(
                "api/ProfileApi/photo",
                content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Upload Error",
                    $"Unable to upload photo.\n{responseContent}",
                    "OK");

                PhotoStatusLabel.Text = "Upload failed.";
                return;
            }

            var result = JsonSerializer.Deserialize<PhotoUploadResponse>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result != null && !string.IsNullOrWhiteSpace(result.ImageUrl))
            {
                _user.ImageUrl = result.ImageUrl;

                var absoluteUrl = new Uri(
                    new Uri(ApiBaseUrl),
                    result.ImageUrl);

                ProfileImage.Source = ImageSource.FromUri(absoluteUrl);

                PhotoStatusLabel.Text = "Photo updated successfully!";
            }
        }
        catch (Exception ex)
        {
            PhotoStatusLabel.Text = "Upload failed.";

            await DisplayAlert("Upload Error", ex.Message, "OK");
        }
    }

    private async void UpdateProfile_Clicked(object sender, EventArgs e)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlert(
                    "Authentication",
                    "Your session has expired. Please log in again.",
                    "OK");

                return;
            }

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

            client.BaseAddress = new Uri(ApiBaseUrl);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new UpdateProfileRequest
            {
                FirstName = FirstNameEntry.Text?.Trim() ?? string.Empty,
                LastName = LastNameEntry.Text?.Trim() ?? string.Empty,
                Address = AddressEntry.Text?.Trim() ?? string.Empty,
                PhoneNumber = PhoneEntry.Text?.Trim() ?? string.Empty
            };

            var json = JsonSerializer.Serialize(request);

            using var content = new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(
                "api/ProfileApi/details",
                content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    $"Update Error ({(int)response.StatusCode})",
                    $"Server response:\n{responseContent}",
                    "OK");

                return;
            }

            // Update local user information
            _user.FirstName = request.FirstName;
            _user.LastName = request.LastName;
            _user.Address = request.Address;
            _user.PhoneNumber = request.PhoneNumber;

            var initial = !string.IsNullOrWhiteSpace(_user.FirstName) ? _user.FirstName[..1].ToUpperInvariant() : "U";

            AvatarInitialLabel.Text = initial;

            await DisplayAlert(
                "Success",
                "Your profile has been updated successfully!",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Update Error",
                ex.Message,
                "OK");
        }
    }


    private async Task LoadProfileImageAsync(string imageUrl)
    {
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

            var absoluteUrl = new Uri(new Uri(ApiBaseUrl), imageUrl);

            var imageBytes = await client.GetByteArrayAsync(absoluteUrl);

            ProfileImage.Source = ImageSource.FromStream(
                () => new MemoryStream(imageBytes));

            ProfileImage.IsVisible = true;
            AvatarInitialLabel.IsVisible = false;

            HeaderAvatarImage.Source = ImageSource.FromStream(
                () => new MemoryStream(imageBytes));

            HeaderAvatarImage.IsVisible = true;
        }
        catch (Exception ex)
        {
            PhotoStatusLabel.Text = $"Unable to load photo: {ex.Message}";
        }
    }


    private async void Home_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private class PhotoUploadResponse
    {
        public string? ImageUrl { get; set; }

        public string? Message { get; set; }
    }

    private class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
