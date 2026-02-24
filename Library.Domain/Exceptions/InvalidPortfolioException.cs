namespace Library.Domain.Exceptions;

public class InvalidPortfolioException : Exception
{
    public InvalidPortfolioException()
        : base("Invalid portfolio.")
    {
    }
}
