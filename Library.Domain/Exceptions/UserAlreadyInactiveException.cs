namespace Library.Domain.Exceptions;

public class UserAlreadyInactiveException : Exception
{
    public UserAlreadyInactiveException()
        : base("User is already inactive.")
    {
    }
}
