namespace SF.PJ03.Task40._7_.Services;

public class GalleryAccessException : Exception
{
    public GalleryAccessException(string message) : base(message) { }
    public GalleryAccessException(string message, Exception innerException) : base(message, innerException) { }
}