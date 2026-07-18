namespace FormBuilder.Domain.Exceptions;

public class ConcurrencyException : ConflictException
{
    public ConcurrencyException(string message) : base(message)
    {
    }
}
