namespace Library.Domain.Exceptions;

public class InvalidUserNameException : Exception
{
    public InvalidUserNameException()
        : base("Invalid user name.")
    {
    }

    public InvalidUserNameException(string message)
        : base(message)
    {
    }
}
