#if ANDROID
using Android.Content;
using Android.Provider;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SF.PJ03.Task40._7_.Models;
using SF.PJ03.Task40._7_.Services;

namespace SF.PJ03.Task40._7_.Platforms.Android.Services;

public class AndroidGalleryService : IGalleryService
{
    public async Task<List<ImageItem>> LoadImagesAsync()
    {
        var results = new List<ImageItem>();
        var context = Platform.AppContext;
        var contentResolver = context.ContentResolver;

        string[] projection = {
                MediaStore.MediaColumns.Id,
                MediaStore.MediaColumns.DisplayName,
                MediaStore.MediaColumns.Data,
                MediaStore.MediaColumns.DateTaken
            };

        var queryUri = MediaStore.Images.Media.ExternalContentUri;
        string sortOrder = MediaStore.MediaColumns.DateTaken + " DESC";

        using var imageCursor = contentResolver.Query(
            queryUri,
            projection,
            null,
            null,
            sortOrder);

        if (imageCursor == null || imageCursor.Count == 0)
            return results;

        if (imageCursor.MoveToFirst())
        {
            int idColumn = imageCursor.GetColumnIndex(projection[0]);
            int displayNameColumn = imageCursor.GetColumnIndex(projection[1]);
            int filePathColumn = imageCursor.GetColumnIndex(projection[2]);
            int dateTakenColumn = imageCursor.GetColumnIndex(projection[3]);

            do
            {
                if (idColumn < 0 || displayNameColumn < 0 || filePathColumn < 0 || dateTakenColumn < 0)
                    continue;

                try
                {
                    var id = imageCursor.GetLong(idColumn);
                    var displayName = imageCursor.GetString(displayNameColumn);
                    var filePath = imageCursor.GetString(filePathColumn);
                    var dateTaken = imageCursor.GetLong(dateTakenColumn);

                    // Проверяем существование файла
                    if (!File.Exists(filePath))
                        continue;

                    var creationDate = DateTimeOffset.FromUnixTimeMilliseconds(dateTaken).DateTime;

                    results.Add(new ImageItem(filePath, displayName, creationDate, id));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing image: {ex.Message}");
                }

            } while (imageCursor.MoveToNext());
        }

        return results;
    }

    public async Task<bool> DeleteImageAsync(ImageItem image)
    {
        var context = Platform.AppContext;
        var contentResolver = context.ContentResolver;

        // Формируем URI для конкретного изображения в MediaStore, используя его ID
        var itemUri = ContentUris.WithAppendedId(MediaStore.Images.Media.ExternalContentUri, image.MediaStoreId);

        try
        {
            int rowsDeleted = contentResolver.Delete(itemUri, null, null);
            return rowsDeleted > 0;
        }
        catch (Java.Lang.SecurityException secEx)
        {
            System.Diagnostics.Debug.WriteLine($"MediaStore delete SecurityException: {secEx.Message}");
            throw new GalleryAccessException("Ошибка безопасности при удалении изображения", secEx);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting image via MediaStore: {ex.Message}");
            throw new GalleryAccessException($"Ошибка удаления изображения: {ex.Message}", ex);
        }
    }
}

#endif