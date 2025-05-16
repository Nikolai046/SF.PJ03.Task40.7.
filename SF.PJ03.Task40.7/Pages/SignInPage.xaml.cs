using SF.PJ03.Task40._7_.Models;
using System.Security.Cryptography;
using System.Text;

namespace SF.PJ03.Task40._7_.Pages;

public partial class SignInPage : ContentPage
{
    private readonly Color _filledColor = Colors.Gray;
    private readonly Color _emptyColor = Colors.Transparent;
    private readonly string? _storedPinHash;
    private readonly Border[] _pinDots;

    public SignInPage()
    {
        InitializeComponent();
        _pinDots = [digit1, digit2, digit3, digit4];
        _storedPinHash = SecureStorage.Default.GetAsync("UserPin").Result;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializePage();
    }

    private async void InitializePage()
    {
        pinEntry.Text = string.Empty;
        UpdatePinDots(string.Empty);
        UpdatePinLabelText();
        Dispatcher.Dispatch(() => pinEntry.Focus());
    }

    private async void OnPinChanged(object? sender, TextChangedEventArgs e)
    {
        var pin = e.NewTextValue ?? string.Empty;
        UpdatePinDots(pin);
        var isPinComplete = pin.Length == 4;

        if (isPinComplete)
        {
            var enteredPinHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pin)));
            var isPinValid = enteredPinHash == _storedPinHash;
            pinLabelText.Text = isPinValid ? "PIN-код принят" : "PIN-код неверный";
            pinLabelText.TextColor = isPinValid ? Colors.Green : Colors.Red;

            if (!isPinValid)
            {
                await Task.Delay(2000);
                InitializePage();
            }
            else
            {
                // Получаем сервис через контейнер зависимостей
                var mauiContext = Application.Current.Handler.MauiContext;
                var galleryService = mauiContext.Services.GetService<IGalleryService>();

                if (galleryService == null)
                {
                    await DisplayAlert("Ошибка", "Не удалось загрузить галерею.", "OK");
                    return;
                }

                // Создаем экземпляр GalleryPage с внедренным сервисом
                var galleryPage = new GalleryPage(galleryService);

                if (Application.Current != null) Application.Current.MainPage = new NavigationPage(galleryPage);
            }
        }
    }

    private void UpdatePinDots(string pin)
    {
        for (int i = 0; i < _pinDots.Length; i++)
        {
            _pinDots[i].BackgroundColor = i < pin.Length ? _filledColor : _emptyColor;
        }
    }

    private void UpdatePinLabelText()
    {
        pinLabelText.Text = "Введите PIN-код";
        pinLabelText.TextColor = Colors.Black;
    }

    private void OnPinFieldTapped(object? sender, EventArgs e)
    {
        pinEntry.Focus();
    }
}