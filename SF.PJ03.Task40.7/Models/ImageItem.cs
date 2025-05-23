﻿using System.ComponentModel;

namespace SF.PJ03.Task40._7_.Models;

/// <summary>
/// Модель, представляющая элемент изображения в галерее. 
/// Содержит информацию о пути к файлу, имени, источнике изображения, дате создания и ID в MediaStore.
/// </summary>
public partial class ImageItem : INotifyPropertyChanged
{
    private string _filePath;

    // Путь к файлу изображения или URI.
    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath == value) return;
            _filePath = value;
            OnPropertyChanged(nameof(FilePath));
        }
    }

    private string _fileName;

    // Имя файла изображения.
    public string FileName
    {
        get => _fileName;
        set
        {
            if (_fileName == value) return;
            _fileName = value;
            OnPropertyChanged(nameof(FileName));
        }
    }

    private ImageSource _source;

    // Источник изображения для отображения.
    public ImageSource Source
    {
        get => _source;
        set
        {
            if (_source == value) return;
            _source = value;
            OnPropertyChanged(nameof(Source));
        }
    }

    private DateTime _creationDate;

    // Дата создания изображения.
    public DateTime CreationDate
    {
        get => _creationDate;
        set
        {
            if (_creationDate == value) return;
            _creationDate = value;
            OnPropertyChanged(nameof(CreationDate));
        }
    }

    // Поле для хранения ID из MediaStore (для Android)
    public long MediaStoreId { get; set; }

    // Метод для загрузки изображения
    public void LoadImage()
    {
        if (!string.IsNullOrEmpty(FilePath))
        {
            Source = ImageSource.FromFile(FilePath);
        }
    }

    // Конструкторы
    public ImageItem()
    { }

    public ImageItem(string filePath, string fileName, DateTime creationDate, long mediaStoreId = 0)
    {
        _filePath = filePath;
        _fileName = fileName;
        _creationDate = creationDate;
        MediaStoreId = mediaStoreId;
        LoadImage();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    // Вызывает событие PropertyChanged для указанного свойства.
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}