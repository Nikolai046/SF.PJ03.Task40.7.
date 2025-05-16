namespace SF.PJ03.Task40._7_.Models;

public class DefaultGalleryService : IGalleryService
{
    public async Task<List<ImageItem>> LoadImagesAsync()
    {
        return [];
    }

    public async Task<bool> DeleteImageAsync(ImageItem? image)
    {
        if (!File.Exists(image.FilePath))
            return false;

        try
        {
            File.Delete(image.FilePath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}