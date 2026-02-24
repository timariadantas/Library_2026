namespace Library.Domain.Exceptions;

public class InvalidUserException : Exception
{
    public InvalidUserException()
        : base("Invalid user.")
    {
    }
}
