using Library.Domain.Entities;
using Library.Domain.Enums;
using NUlid;

namespace Library.Application.Services;

public interface ILoanService
{
    void CreateLoan(Ulid portfolioId, Ulid userId, BookCondition condition, int period);

    Loan? GetLoanById(Ulid id);

    IEnumerable<Loan> GetAllLoans();

    IEnumerable<Loan> GetLoansByUser(Ulid userId);

    void ReturnLoan(Ulid id, BookCondition condition);

    void DeleteLoan(Ulid id);
}
