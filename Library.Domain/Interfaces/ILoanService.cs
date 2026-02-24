namespace Library.Domain.Interfaces;

public interface ILoanRepository
{
    bool ExistsActiveLoan(string isbn);
}
