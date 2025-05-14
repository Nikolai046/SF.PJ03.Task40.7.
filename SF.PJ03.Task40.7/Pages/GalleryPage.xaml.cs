using System.Collections.ObjectModel;
using System.ComponentModel;
using SF.PJ03.Task40._7_.Models;
#if ANDROID
using Android.Provider;
using Android.Database;
using Android.Content;
#endif

namespace SF.PJ03.Task40._7_.Pages;

public partial class GalleryPage : ContentPage, INotifyPropertyChanged
{
    private ObservableCollection<ImageItem> _images;
    public ObservableCollection<ImageItem> Images
    {
        get => _images;
        set
        {
            _images = value;
            OnPropertyChanged(nameof(Images));
        }
    }

    private ImageItem _selectedImage;
    public ImageItem SelectedImage
    {
        get => _selectedImage;
        set
        {
            _selectedImage = value;
            OnPropertyChanged(nameof(SelectedImage));
            // Можно добавить логику для активации/деактивации кнопок
        }
    }

    public GalleryPage()
    {
        InitializeComponent();
        Images = new ObservableCollection<ImageItem>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RequestAndLoadImages();
    }

    private async Task RequestAndLoadImages()
    {
        var readPermission = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (readPermission != PermissionStatus.Granted)
        {
            readPermission = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        // WRITE_EXTERNAL_STORAGE - для MediaStore.delete менее критично, чем READ_EXTERNAL_STORAGE для чтения.
        // Но оставим на случай, если какие-то операции его все же требуют на старых API.
        var writePermission = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (writePermission != PermissionStatus.Granted)
        {
            writePermission = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        if (readPermission == PermissionStatus.Granted)
        {
            LoadImagesFromCameraRoll(); // Теперь использует MediaStore
        }
        else
        {
            await DisplayAlert("Нет разрешения", "Не удалось получить доступ к хранилищу для загрузки изображений.", "OK");
        }
    }

    private void LoadImagesFromCameraRoll()
    {
        Images.Clear();

#if ANDROID
            Images.Clear();
            var context = Platform.AppContext; 
            var contentResolver = context.ContentResolver;

            string[] projection = {
                Android.Provider.MediaStore.MediaColumns.Id,
                Android.Provider.MediaStore.MediaColumns.DisplayName,
                Android.Provider.MediaStore.MediaColumns.Data, 
                Android.Provider.MediaStore.MediaColumns.DateTaken
            };

            var queryUri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
            
            string selection = Android.Provider.MediaStore.Images.ImageColumns.BucketDisplayName + " = ?";
            string[] selectionArgs = { "Camera" }; 

            string sortOrder = Android.Provider.MediaStore.MediaColumns.DateTaken + " DESC";

            ICursor imageCursor = null;
            try
            {
                imageCursor = contentResolver.Query(queryUri, projection, selection, selectionArgs, sortOrder);

                if (imageCursor != null)
                {
                    int idColumn = imageCursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.MediaColumns.Id);
                    int displayNameColumn = imageCursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.MediaColumns.DisplayName);
                    int dataColumn = imageCursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.MediaColumns.Data);
                    int dateTakenColumn = imageCursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.MediaColumns.DateTaken);

                    while (imageCursor.MoveToNext())
                    {
                        long id = imageCursor.GetLong(idColumn);
                        string displayName = imageCursor.GetString(displayNameColumn);
                        string filePath = imageCursor.GetString(dataColumn); 
                        long dateTakenMillis = imageCursor.GetLong(dateTakenColumn);

                        DateTime creationDate = DateTimeOffset.FromUnixTimeMilliseconds(dateTakenMillis).LocalDateTime;
                        Android.Net.Uri itemUri = Android.Content.ContentUris.WithAppendedId(queryUri, id); 

                        if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                        {
                            Images.Add(new ImageItem
                            {
                                MediaStoreId = id,
                                FilePath = filePath, 
                                FileName = displayName,
                                Source = ImageSource.FromStream(() => contentResolver.OpenInputStream(itemUri)),
                                CreationDate = creationDate
                            });
                        }
                        else
                        {
                             System.Diagnostics.Debug.WriteLine($"Skipping MediaStore item. FilePath: {filePath}, ItemUri: {itemUri}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MediaStore query returned null cursor for Camera bucket.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading images from MediaStore: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Ошибка MediaStore", $"Не удалось загрузить изображения: {ex.Message}", "OK");
                });
            }
            finally
            {
                imageCursor?.Close(); 
            }
            if (!Images.Any())
            {
                 System.Diagnostics.Debug.WriteLine("No images found in Camera bucket via MediaStore or an error occurred.");
            }
#else
        // Код для других платформ или если ANDROID не определен
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await DisplayAlert("Не реализовано", "Загрузка изображений через MediaStore доступна только для Android.", "OK");
        });
#endif
    }


    private async void OpenButton_Clicked(object sender, EventArgs e)
    {
        if (SelectedImage != null)
        {
            await Navigation.PushAsync(new ImageViewerPage(SelectedImage.FilePath, SelectedImage.CreationDate));
        }
        else
        {
            await DisplayAlert("Не выбрано", "Пожалуйста, выберите изображение для открытия.", "OK");
        }
    }


    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (SelectedImage == null)
        {
            await DisplayAlert("Не выбрано", "Пожалуйста, выберите изображение для удаления.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Удалить изображение?", $"Вы уверены, что хотите удалить {SelectedImage.FileName}?", "Да", "Нет");
        if (!confirm) return;

#if ANDROID
            var context = Platform.AppContext;
            var contentResolver = context.ContentResolver;
            // Формируем URI для конкретного изображения в MediaStore, используя его ID
            Android.Net.Uri itemUri = ContentUris.WithAppendedId(MediaStore.Images.Media.ExternalContentUri, SelectedImage.MediaStoreId);

            try
            {
                // Пытаемся удалить изображение через ContentResolver
                // Для Android 10 (API 29) это может выбросить RecoverableSecurityException, если у приложения нет прав
                // на удаление этого файла. Для Android 11+ (API 30) предпочтительнее MediaStore.createDeleteRequest,
                // но это значительно усложнит код (требует обработки ActivityResult).
                // Этот упрощенный вариант попытается удалить и сообщит об ошибке.
                int rowsDeleted = contentResolver.Delete(itemUri, null, null);

                if (rowsDeleted > 0)
                {
                    Images.Remove(SelectedImage);
                    SelectedImage = null;
                    await DisplayAlert("Удалено", "Изображение успешно удалено.", "OK");
                }
                else
                {
                    // Если rowsDeleted == 0, файл мог быть уже удален, или нет прав.
                    // Проверим, существует ли он еще по старому пути (хотя это менее надежно)
                    if (!System.IO.File.Exists(SelectedImage.FilePath) && Images.Contains(SelectedImage))
                    {
                        // Если файла нет и он в списке, просто убираем из списка
                        Images.Remove(SelectedImage);
                        SelectedImage = null;
                        await DisplayAlert("Удалено", "Изображение удалено (или уже было удалено).", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Не удалось удалить изображение через MediaStore (возможно, нет прав или файл уже удален).", "OK");
                    }
                }
            }
            catch (Java.Lang.SecurityException secEx) // Явно ловим SecurityException
            {
                System.Diagnostics.Debug.WriteLine($"MediaStore delete SecurityException: {secEx.Message}");
                // На Android 10 это может быть RecoverableSecurityException.
                // Для упрощения просто выводим сообщение.
                await DisplayAlert("Ошибка безопасности", $"Не удалось удалить изображение: {secEx.Message}. Приложению может требоваться разрешение на изменение этого файла.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image via MediaStore: {ex.Message}");
                await DisplayAlert("Ошибка удаления", $"Не удалось удалить изображение: {ex.Message}", "OK");
            }
#else
        // Код для других платформ (ваш предыдущий File.Delete)
        try
        {
            if (File.Exists(SelectedImage.FilePath))
            {
                File.Delete(SelectedImage.FilePath);
                Images.Remove(SelectedImage);
                SelectedImage = null;
                await DisplayAlert("Удалено", "Изображение успешно удалено (файловая система).", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Файл не найден.", "OK");
                if (Images.Contains(SelectedImage)) Images.Remove(SelectedImage);
                SelectedImage = null;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка удаления", $"Не удалось удалить изображение: {ex.Message}", "OK");
        }
#endif
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

