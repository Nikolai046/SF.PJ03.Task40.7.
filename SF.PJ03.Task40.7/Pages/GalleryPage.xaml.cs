﻿using SF.PJ03.Task40._7_.Models;
using SF.PJ03.Task40._7_.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SF.PJ03.Task40._7_.Pages;

/// <summary>
/// Страница для отображения галереи изображений. 
/// Позволяет просматривать, выбирать, открывать и удалять изображения.
/// </summary>
public partial class GalleryPage : ContentPage
{
    private readonly IGalleryService _galleryService;

    private ObservableCollection<ImageItem?> _images = null!;

    // Коллекция изображений для отображения в галерее.
    public ObservableCollection<ImageItem?> Images
    {
        get => _images;
        set
        {
            if (_images == value) return;
            _images = value;
            OnPropertyChanged(nameof(Images));
        }
    }

    private ImageItem? _selectedImage;

    // Выбранное в данный момент изображение в галерее.
    public ImageItem? SelectedImage
    {
        get => _selectedImage;
        set
        {
            if (_selectedImage == value) return;
            _selectedImage = value;
            OnPropertyChanged(nameof(SelectedImage));
            OnPropertyChanged(nameof(IsImageSelected));
            ImageCollectionView.SelectedItem = value;
        }
    }

    private bool _isLoading;

    // Указывает, происходит ли в данный момент загрузка изображений.
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading == value) return;
            _isLoading = value;
        }
    }

    // Команда для выбора изображения.
    public ICommand SelectImageCommand { get; }

    // Указывает, выбрано ли какое-либо изображение.
    public bool IsImageSelected => SelectedImage != null;

    // Инициализирует новый экземпляр страницы галереи с указанным сервисом галереи.
    public GalleryPage(IGalleryService galleryService)
    {
        InitializeComponent();
        _galleryService = galleryService;
        Images = [];
        BindingContext = this;
        SelectImageCommand = new Command(ExecuteSelectImage);
    }

    // Обрабатывает команду выбора изображения.
    private void ExecuteSelectImage(object param)
    {
        if (param is ImageItem image)
        {
            SelectedImage = image;
        }
    }

    // Вызывается при появлении страницы, инициирует загрузку изображений.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RequestAndLoadImages();
    }

    // Запрашивает необходимые разрешения и загружает изображения.
    private async Task RequestAndLoadImages()
    {
        IsLoading = true;

        try
        {
            // Для Android 13+ (API 33+) используем новые разрешения
            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 13)
            {
                var readMediaResult = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (readMediaResult != PermissionStatus.Granted)
                {
                    readMediaResult = await Permissions.RequestAsync<Permissions.Photos>();
                }

                if (readMediaResult != PermissionStatus.Granted)
                {
                    await DisplayAlert("Требуется доступ", "Разрешите доступ к фотографиям для работы с галереей", "OK");
                    return;
                }
            }
            else // Для старых версий Android
            {
                var readResult = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (readResult != PermissionStatus.Granted)
                {
                    readResult = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                var writeResult = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (writeResult != PermissionStatus.Granted)
                {
                    writeResult = await Permissions.RequestAsync<Permissions.StorageWrite>();
                }

                if (readResult != PermissionStatus.Granted || writeResult != PermissionStatus.Granted)
                {
                    await DisplayAlert("Требуется доступ", "Разрешите доступ к хранилищу для работы с галереей", "OK");
                    return;
                }
            }

            await LoadImages();
        }
        catch (PermissionException pex)
        {
            await DisplayAlert("Ошибка доступа", $"Не удалось получить разрешения: {pex.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка при загрузке изображений: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Загружает изображения с использованием сервиса галереи.
    private async Task LoadImages()
    {
        // Очищаем предыдущие изображения
        Images.Clear();
        SelectedImage = null;

        try
        {
            var images = await _galleryService.LoadImagesAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var image in images)
                {
                    Images.Add(image);
                }
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить изображения: {ex.Message}", "OK");
            });
        }
    }

    // Обрабатывает изменение выбранного элемента в коллекции изображений.
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedImage = e.CurrentSelection.Count > 0 ? e.CurrentSelection[0] as ImageItem : null;
    }

    // Обрабатывает нажатие кнопки "Открыть", переходит на страницу просмотра изображения.
    private async void OpenButton_Clicked(object sender, EventArgs e)
    {
        if (SelectedImage == null)
        {
            await DisplayAlert("Не выбрано", "Пожалуйста, выберите изображение для открытия.", "OK");
            return;
        }

        if (!File.Exists(SelectedImage.FilePath))
        {
            await DisplayAlert("Ошибка", "Файл изображения не найден или недоступен.", "OK");
            Images.Remove(SelectedImage);
            SelectedImage = null;
            return;
        }

        await Navigation.PushAsync(new ImageViewerPage(SelectedImage.FilePath, SelectedImage.CreationDate));
    }

    // Обрабатывает нажатие кнопки "Удалить", удаляет выбранное изображение после подтверждения.
    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (SelectedImage == null)
        {
            await DisplayAlert("Не выбрано", "Пожалуйста, выберите изображение для удаления.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Удалить изображение?",
            $"Вы уверены, что хотите удалить {SelectedImage.FileName}?", "Да", "Нет");

        if (!confirm) return;

        try
        {
            bool deleted = await _galleryService.DeleteImageAsync(SelectedImage);

            if (deleted)
            {
                Images.Remove(SelectedImage);
                SelectedImage = null;
                await DisplayAlert("Удалено", "Изображение успешно удалено.", "OK");
            }
            else
            {
                // Проверяем, существует ли файл физически
                if (!File.Exists(SelectedImage.FilePath))
                {
                    Images.Remove(SelectedImage);
                    SelectedImage = null;
                    await DisplayAlert("Обновлено", "Изображение удалено из списка (файл не существует).", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить изображение.", "OK");
                }
            }
        }
        catch (GalleryAccessException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка удаления", $"Не удалось удалить изображение: {ex.Message}", "OK");
        }
    }
}