using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SF.PJ03.Task40._7_.Pages;

public partial class SignInPage : ContentPage
{
    private readonly Color _filledColor = Colors.Gray;
    private readonly Color _emptyColor = Colors.Transparent;
    private string? storedPinHash;
    private readonly Border[] _pinDots;

    public SignInPage()
    {
        InitializeComponent();
        _pinDots = [digit1, digit2, digit3, digit4];
        storedPinHash = SecureStorage.Default.GetAsync("UserPin").Result;
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
            var isPinValid = enteredPinHash == storedPinHash;
            pinLabelText.Text = isPinValid ? "PIN-код принят" : "PIN-код неверный";
            pinLabelText.TextColor = isPinValid ? Colors.Green : Colors.Red;

            if (!isPinValid)
            {
                await Task.Delay(2000);
                InitializePage();
            }
            else
            {
                this.Title = "Запускаем приложение";
                await Shell.Current.GoToAsync("//GalleryPage");
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