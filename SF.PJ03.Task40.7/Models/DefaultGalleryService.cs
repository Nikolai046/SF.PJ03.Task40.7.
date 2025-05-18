namespace SF.PJ03.Task40._7_.Models;

/// <summary>
/// Реализация сервиса галереи по умолчанию. 
/// Используется для платформ, отличных от Android, или как заглушка. Позволяет удалять файлы по прямому пути.
/// </summary>
public class DefaultGalleryService : IGalleryService
{
    // Загружает изображения (базовая реализация возвращает пустой список).
    public async Task<List<ImageItem>> LoadImagesAsync()
    {
        return [];
    }

    // Удаляет изображение по указанному пути к файлу.
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