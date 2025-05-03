namespace SF.PJ03.Task40._7_.Pages
{
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            InitializePage();
        }

        private async void InitializePage()
        {
            submitButton.IsVisible = true;
            _nonConfirmedPin = null;
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

            submitButton.IsEnabled = isPinComplete && _nonConfirmedPin == null;

            if (isPinComplete && _nonConfirmedPin != null)
            {
                var isPinValid = pin == _nonConfirmedPin;
                pinLabelText.Text = isPinValid ? "PIN-код принят" : "PIN-код не совпадает";
                pinLabelText.TextColor = isPinValid ? Colors.Green : Colors.Red;

                if (!isPinValid)
                {
                    await Task.Delay(2000);
                    InitializePage();
                }
                await SecureStorage.Default.SetAsync("UserPin", pin);
                this.Title = "Обновленный заголовок";
            }
        }

        private void UpdatePinDots(string pin)
        {
            for (int i = 0; i < _pinDots.Length; i++)
            {
                _pinDots[i].BackgroundColor = i < pin.Length ? _filledColor : _emptyColor;
            }
        }

        private void SubmitButton_OnClicked(object? sender, EventArgs e)
        {
            _nonConfirmedPin = pinEntry.Text;
            pinEntry.Text = string.Empty;
            UpdatePinDots(string.Empty);
            UpdatePinLabelText();
            submitButton.IsVisible = false;
            pinEntry.Focus();
        }

        private void UpdatePinLabelText()
        {
            pinLabelText.Text = _nonConfirmedPin == null ? "Придумайте PIN-код" : "Подтвердите PIN-код";
            pinLabelText.TextColor = Colors.Black;
        }

        private void OnPinFieldTapped(object? sender, EventArgs e)
        {
            pinEntry.Focus();
        }
    }
}