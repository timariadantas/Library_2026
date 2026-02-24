namespace Library.Domain.Exceptions;

public class UserAlreadyActiveException : Exception
{
    public UserAlreadyActiveException()
        : base("User is already active.")
    {
    }
}
