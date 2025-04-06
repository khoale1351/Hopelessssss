using SixLabors.ImageSharp;

namespace Travel.Repositories.IMAGESERVICE
{
    public interface IImageService
    {
        Task<(bool IsSuccess, string? FilePath, string? ErrorMessage)> SaveImageAsync(
            IFormFile imageFile,
            string subFolder,
            string filePrefix = "image",
            string? oldFilePath = null,
            Size? targetSize = null
        );
    }
}
