namespace SF.PJ03.Task40._7_.Services;

/// <summary>
/// Пользовательское исключение, выбрасываемое при ошибках доступа к галерее или операциях с изображениями.
/// </summary>
public class GalleryAccessException : Exception
{
    // Инициализирует новый экземпляр класса GalleryAccessException с указанным сообщением об ошибке.
    public GalleryAccessException(string message) : base(message)
    {
    }

    // Инициализирует новый экземпляр класса GalleryAccessException с указанным сообщением об ошибке и ссылкой на внутреннее исключение.
    public GalleryAccessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}