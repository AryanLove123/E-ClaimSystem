namespace EClaim.Domain.Exceptions;

public class ForbiddenAccessException : DomainException
{
    public ForbiddenAccessException(string message = "You do not have access to this resource.")
        : base(message) { }
}
