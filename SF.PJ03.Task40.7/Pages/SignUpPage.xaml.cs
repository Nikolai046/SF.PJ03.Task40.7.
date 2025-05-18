using SF.PJ03.Task40._7_.Models;
using System.Security.Cryptography;
using System.Text;

namespace SF.PJ03.Task40._7_.Pages
{
    /// <summary>
    /// Страница для первоначальной регистрации пользователя путем создания и подтверждения PIN-кода.
    /// </summary>
    public partial class SignUpPage : ContentPage
    {
        private readonly Color _filledColor = Colors.Gray;
        private readonly Color _emptyColor = Colors.Transparent;
        private string? _nonConfirmedPin;
        private readonly Border[] _pinDots;

        public SignUpPage()
        {
            InitializeComponent();
            _pinDots = [digit1, digit2, digit3, digit4];
        }

        // Вызывается при появлении страницы, инициализирует элементы управления.
        protected override void OnAppearing()
        {
            base.OnAppearing();
            InitializePage();
        }

        // Сбрасывает состояние страницы и поля ввода PIN-кода для нового ввода или подтверждения.
        private async void InitializePage()
        {
            submitButton.IsVisible = true;
            _nonConfirmedPin = null;
            pinEntry.Text = string.Empty;
            UpdatePinDots(string.Empty);
            UpdatePinLabelText();
            Dispatcher.Dispatch(() => pinEntry.Focus());
        }

        // Обрабатывает изменение текста в поле ввода PIN-кода, управляет логикой подтверждения PIN.
        private async void OnPinChanged(object? sender, TextChangedEventArgs e)
        {
            var pin = e.NewTextValue ?? string.Empty;
            UpdatePinDots(pin);
            var isPinComplete = pin.Length == 4;

            submitButton.IsEnabled = isPinComplete && _nonConfirmedPin == null;

            if (isPinComplete && _nonConfirmedPin != null)
            {
                var isPinValid = pin == _nonConfirmedPin;
                pinLabelText.Text = isPinValid ? "PIN-код сохранен" : "PIN-код не совпадает";
                pinLabelText.TextColor = isPinValid ? Colors.Green : Colors.Red;

                if (!isPinValid)
                {
                    await Task.Delay(2000);
                    InitializePage();
                }
                else
                {
                    // Создаем хэш
                    var pinHashинBytes = SHA256.HashData(Encoding.UTF8.GetBytes(pin));
                    var pinHash = Convert.ToHexString(pinHashинBytes);
                    await SecureStorage.Default.SetAsync("UserPin", pinHash);
                    await Task.Delay(2000);
                    this.Title = "Запускаем приложение";

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

        // Обновляет цвет точек PIN-кода в зависимости от введенного значения.
        private void UpdatePinDots(string pin)
        {
            for (int i = 0; i < _pinDots.Length; i++)
            {
                _pinDots[i].BackgroundColor = i < pin.Length ? _filledColor : _emptyColor;
            }
        }

        // Обрабатывает нажатие кнопки "Отправить", сохраняет введенный PIN-код и очищает поле ввода.
        private void SubmitButton_OnClicked(object? sender, EventArgs e)
        {
            _nonConfirmedPin = pinEntry.Text;
            pinEntry.Text = string.Empty;
            UpdatePinDots(string.Empty);
            UpdatePinLabelText();
            submitButton.IsVisible = false;
            pinEntry.Focus();
        }

        // Обновляет текст метки PIN-кода в зависимости от состояния подтверждения.
        private void UpdatePinLabelText()
        {
            pinLabelText.Text = _nonConfirmedPin == null ? "Придумайте PIN-код" : "Подтвердите PIN-код";
            pinLabelText.TextColor = Colors.Black;
        }

        // Обрабатывает событие нажатия на поле ввода PIN-кода, чтобы установить фокус.
        private void OnPinFieldTapped(object? sender, EventArgs e)
        {
            pinEntry.Focus();
        }
    }
}