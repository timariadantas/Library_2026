namespace Library.Domain.Exceptions;

public class InactiveUserException : DomainException
{
    public InactiveUserException()
        : base("User is inactive")
    {
    }
}
