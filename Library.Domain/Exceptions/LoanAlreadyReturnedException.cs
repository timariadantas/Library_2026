namespace Library.Domain.Exceptions;

public class LoanAlreadyReturnedException : Exception
{
    public LoanAlreadyReturnedException()
        : base("Loan already returned.")
    {
    }
}
