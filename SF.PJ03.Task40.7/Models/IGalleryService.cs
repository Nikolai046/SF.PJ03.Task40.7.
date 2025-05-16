namespace SF.PJ03.Task40._7_.Models;

public interface IGalleryService
{
    Task<List<ImageItem>> LoadImagesAsync();

    Task<bool> DeleteImageAsync(ImageItem? image);
}