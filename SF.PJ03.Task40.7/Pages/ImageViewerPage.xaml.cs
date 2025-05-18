namespace SF.PJ03.Task40._7_.Pages;

/// <summary>
/// Страница для просмотра полноразмерного изображения и его даты создания.
/// </summary>
public partial class ImageViewerPage : ContentPage
{
    public ImageViewerPage(string imagePath, DateTime creationDate)
    {
        InitializeComponent();
        FullImage.Source = ImageSource.FromFile(imagePath);
        CreationDateLabel.Text = $"Сделано: {creationDate:dd.MM.yyyy HH:mm:ss}";
    }
}