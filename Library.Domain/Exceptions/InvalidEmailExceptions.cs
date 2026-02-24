namespace Library.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException()
        : base("Email is invalid")
    {
    }
}
