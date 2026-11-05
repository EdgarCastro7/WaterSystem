namespace WaterSystem.Helpers
{
    public class ImageHelper : IImageHelper
    {
        public async Task<string> UploadImageAsync(IFormFile imageFile, string folder)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("No image was provided.");
            }

            string extension = Path.GetExtension(imageFile.FileName).ToLower();

            if (extension != ".jpg" && extension != ".jpeg" &&
                extension != ".png" && extension != ".jfif")
            {
                throw new ArgumentException("Invalid image format.");
            }

            string guid = Guid.NewGuid().ToString();
            string file = $"{guid}{extension}";

            string directory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                folder);

            Directory.CreateDirectory(directory);

            string path = Path.Combine(directory, file);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folder}/{file}";
        }
    }
}
