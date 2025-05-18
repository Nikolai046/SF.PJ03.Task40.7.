#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using SF.PJ03.Task40._7_.Models;
using SF.PJ03.Task40._7_.Services;

namespace SF.PJ03.Task40._7_.Platforms.Android.Services
{
    /// <summary>
    /// Реализация сервиса галереи для платформы Android, предоставляющая методы для загрузки и удаления изображений из MediaStore.
    /// </summary>
    public class AndroidGalleryService : IGalleryService
    {
        // Асинхронно загружает список изображений из MediaStore устройства.
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
            string sortOrder = MediaStore.IMediaColumns.DateTaken + " DESC";

            await Task.Run(() =>
            {
                using var imageCursor = contentResolver.Query(
                    queryUri,
                    projection,
                    null,
                    null,
                    sortOrder);

                if (imageCursor == null || imageCursor.Count == 0)
                    return;

                if (imageCursor.MoveToFirst())
                {
                    var idColumn = imageCursor.GetColumnIndex(projection[0]);
                    var displayNameColumn = imageCursor.GetColumnIndex(projection[1]);
                    var filePathColumn = imageCursor.GetColumnIndex(projection[2]); // MediaStore.IMediaColumns.Data
                    var dateTakenColumn = imageCursor.GetColumnIndex(projection[3]);

                    do
                    {
                        if (idColumn < 0 || displayNameColumn < 0 || dateTakenColumn < 0)
                            continue;

                        try
                        {
                            var id = imageCursor.GetLong(idColumn);
                            var displayName = imageCursor.GetString(displayNameColumn);
                            var filePath = filePathColumn >= 0 ? imageCursor.GetString(filePathColumn) : null; // Может быть null
                            var dateTaken = imageCursor.GetLong(dateTakenColumn);

                            // Проверка существования файла по filePath может быть ненадежной в Scoped Storage.
                            // Если filePath не null и файл не существует, пропускаем.
                            if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
                            {
                                System.Diagnostics.Debug.WriteLine($"Путь к файлу, указанный в MediaStore, не существует: {filePath}");
                                // continue; // если нужно пропускать такие файлы
                            }

                            var creationDate = DateTimeOffset.FromUnixTimeMilliseconds(dateTaken).DateTime;
                            results.Add(new ImageItem(filePath ?? $"content://media/external/images/media/{id}", displayName, creationDate, id));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка обработки изображения из MediaStore: {ex.Message}");
                        }

                    } while (imageCursor.MoveToNext());
                }
            });

            return results;
        }

        // Асинхронно удаляет указанное изображение из MediaStore, запрашивая подтверждение пользователя для Android 11+.
        public async Task<bool> DeleteImageAsync(ImageItem? image)
        {
            if (image == null)
            {
                System.Diagnostics.Debug.WriteLine("DeleteImageAsync: ImageItem равен null.");
                return false;
            }

            var context = Platform.AppContext;
            if (context == null)
            {
                System.Diagnostics.Debug.WriteLine("DeleteImageAsync: Platform.AppContext равен null.");
                throw new InvalidOperationException("Application context is not available.");
            }

            var contentResolver = context.ContentResolver;
            if (contentResolver == null)
            {
                System.Diagnostics.Debug.WriteLine("DeleteImageAsync: ContentResolver is null.");
                throw new InvalidOperationException("ContentResolver is not available.");
            }

            // Формируем URI для конкретного изображения в MediaStore, используя его ID
            var itemUri = ContentUris.WithAppendedId(MediaStore.Images.Media.ExternalContentUri, image.MediaStoreId);
            if (itemUri == null)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteImageAsync: Не удалось создать URI для MediaStoreId. {image.MediaStoreId}.");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Попытка удалить изображение с URI: {itemUri}");

            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11 (API 30) и выше
                {
                    var urisToDelete = new List<global::Android.Net.Uri> { itemUri };
                    PendingIntent pendingIntent = MediaStore.CreateDeleteRequest(contentResolver, urisToDelete);

                    if (Platform.CurrentActivity == null)
                    {
                        System.Diagnostics.Debug.WriteLine("DeleteImageAsync: Platform.CurrentActivity is null. Cannot launch PendingIntent.");
                        throw new InvalidOperationException("Current activity is not available to launch delete confirmation.");
                    }

                    bool deleteResult = await SF.PJ03.Task40._7_.Platforms.Android.Helpers.PendingIntentRequester.RequestDeleteAsync(Platform.CurrentActivity, pendingIntent);

                    if (deleteResult)
                    {
                        System.Diagnostics.Debug.WriteLine($"Image deleted successfully via PendingIntent: {itemUri}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Image deletion was cancelled or failed via PendingIntent: {itemUri}");
                    }
                    return deleteResult;
                }
                else // Для версий Android ниже 11 (API < 30)
                {
                    var rowsDeleted = contentResolver.Delete(itemUri, null, null);
                    if (rowsDeleted > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Image deleted successfully (pre-API 30): {itemUri}, rows: {rowsDeleted}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete image (pre-API 30) or image not found: {itemUri}, rows: {rowsDeleted}");
                    }
                    return rowsDeleted > 0;
                }
            }
            catch (Java.Lang.SecurityException secEx)
            {
                System.Diagnostics.Debug.WriteLine($"MediaStore delete SecurityException: {secEx.Message} for URI: {itemUri}");
                string message = "Ошибка безопасности при удалении изображения.";
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    message += " Для Android 11+ возможно потребовалось подтверждение пользователя, но произошла ошибка безопасности. Проверьте разрешения или состояние файла.";
                }
                throw new GalleryAccessException(message, secEx);
            }
            catch (ActivityNotFoundException anfEx) // Если PendingIntent не может быть запущен
            {
                System.Diagnostics.Debug.WriteLine($"ActivityNotFoundException for delete request: {anfEx.Message} for URI: {itemUri}");
                throw new GalleryAccessException("Не удалось запустить системный диалог для подтверждения удаления.", anfEx);
            }
            catch (Exception ex) // Общий перехват других исключений
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image via MediaStore: {ex.GetType().Name} - {ex.Message} for URI: {itemUri}");
                throw new GalleryAccessException($"Произошла неожиданная ошибка при удалении изображения: {ex.Message}", ex);
            }
        }
    }
}
#endif