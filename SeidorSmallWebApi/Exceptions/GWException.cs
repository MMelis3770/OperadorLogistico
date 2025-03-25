namespace SeidorSmallWebApi.Exceptions;

/// <summary>
/// Represents a Gateway exception.
/// </summary>
public class GWException : Exception
{
    public int Code { get; set; }

    internal GWException(int code, string message) : base(message)
    {
        Code = code;
    }

    internal GWException(int code, string message, Exception innerException) : base(message, innerException)
    {
        Code = code;
    }
}