namespace Library.Domain.Exceptions;

public class InvalidIsbnException : Exception
{
    public InvalidIsbnException(string v)
        : base("ISBN Invalid ISBN")
    {
    }
}
