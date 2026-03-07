using Library.Domain.Entities;
using Library.Domain.Repositories;
using Library.Domain.Enums;
using NUlid;

namespace Library.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;

    public LoanService(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public void CreateLoan(Ulid portfolioId, Ulid userId, BookCondition condition, int period)
    {
        var loan = new Loan(
            portfolioId,
            userId,
            condition,
            period
        );

        _loanRepository.Create(loan);
    }

    public Loan? GetLoanById(Ulid id)
    {
        return _loanRepository.GetById(id);
    }

    public IEnumerable<Loan> GetAllLoans()
    {
        return _loanRepository.GetAll();
    }

    public IEnumerable<Loan> GetLoansByUser(Ulid userId)
    {
        return _loanRepository.GetByUserId(userId);
    }

    public void ReturnLoan(Ulid id, BookCondition condition)
    {
        var loan = _loanRepository.GetById(id);

        if (loan == null)
            throw new Exception("Loan not found");

        loan.Return(condition);  //  usando a regra de domínio da entidade.

        _loanRepository.UpdateReturn(loan);
    }

    public void DeleteLoan(Ulid id)
    {
        _loanRepository.Delete(id);
    }
}