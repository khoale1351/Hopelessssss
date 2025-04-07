using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Travel.Repositories.IMAGESERVICE
{
    public class ImageService : IImageService
    {
        public async Task<(bool IsSuccess, string? FilePath, string? ErrorMessage)> SaveImageAsync(
            IFormFile imageFile,
            string subFolder,
            string filePrefix = "image",
            string? oldFilePath = null,
            Size? targetSize = null
        )
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return (false, null, "Chỉ cho phép định dạng ảnh: jpg, jpeg, png, gif.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{filePrefix}-{DateTime.Now.Ticks}.jpg";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var virtualPath = Path.Combine(subFolder, uniqueFileName).Replace("\\", "/");

            using (var image = await Image.LoadAsync(imageFile.OpenReadStream()))
            {
                if (targetSize != null)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = targetSize.Value,
                        Mode = ResizeMode.Crop
                    }));
                }

                var encoder = new JpegEncoder { Quality = 85 };
                await image.SaveAsync(filePath, encoder);
            }

            // Xóa ảnh cũ nếu có
            if (!string.IsNullOrEmpty(oldFilePath)) 
            {
                var oldPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldFilePath);
                if (System.IO.File.Exists(oldPhysicalPath))
                {
                    System.IO.File.Delete(oldPhysicalPath);
                }
            }

            return (true, virtualPath, null);
        }
    }
}
