namespace Library.Domain.Exceptions;

public class InvalidBookConditionException : Exception
{
    public InvalidBookConditionException()
        : base("Invalid book condition.")
    {
    }
}

