namespace EClaim.Domain.Exceptions;

public class InvalidWorkflowTransitionException : DomainException
{
    public InvalidWorkflowTransitionException(string message) : base(message) { }
}