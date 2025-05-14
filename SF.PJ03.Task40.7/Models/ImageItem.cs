using System.ComponentModel;

namespace SF.PJ03.Task40._7_.Models;

public class ImageItem : INotifyPropertyChanged
{
    private string _filePath; // Может быть полезен для информации или как запасной вариант
    public string FilePath
    {
        get => _filePath;
        set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
    }

    private string _fileName;
    public string FileName
    {
        get => _fileName;
        set { _fileName = value; OnPropertyChanged(nameof(FileName)); }
    }

    private ImageSource _source;
    public ImageSource Source
    {
        get => _source;
        set { _source = value; OnPropertyChanged(nameof(Source)); }
    }

    private DateTime _creationDate;
    public DateTime CreationDate
    {
        get => _creationDate;
        set { _creationDate = value; OnPropertyChanged(nameof(CreationDate)); }
    }

    // Поле для хранения ID из MediaStore (только для Android)
    public long MediaStoreId { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}