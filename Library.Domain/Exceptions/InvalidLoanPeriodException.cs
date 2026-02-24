namespace Library.Domain.Exceptions;

public class InvalidLoanPeriodException : Exception
{
    public InvalidLoanPeriodException()
        : base("Invalid loan period.")
    {
    }
}
