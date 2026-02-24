using NUlid;
using Library.Domain.Enums;
using Library.Domain.Exceptions;

namespace Library.Domain.Entities;

public class Loan
{
    public Ulid Id { get; private set; }

    public Ulid PortfolioId { get; private set; }

    public Ulid UserId { get; private set; }

    public DateTime StartAt { get; private set; }

    public DateTime? ReturnAt { get; private set; }

    public int Period { get; private set; }

    public BookCondition LoanCondition { get; private set; }

    public BookCondition? ReturnCondition { get; private set; }


    public Loan(
        Ulid portfolioId,
        Ulid userId,
        BookCondition loanCondition,
        int period = 30)
    {
        Validate(portfolioId, userId, period);

        Id = Ulid.NewUlid();
        PortfolioId = portfolioId;
        UserId = userId;
        StartAt = DateTime.UtcNow;
        Period = period;
        LoanCondition = loanCondition;
    }


    private void Validate(Ulid portfolioId, Ulid userId, int period)
    {
        if (portfolioId == Ulid.Empty)
            throw new InvalidPortfolioException();

        if (userId == Ulid.Empty)
            throw new InvalidUserException();

        if (period <= 0)
            throw new InvalidLoanPeriodException();
    }


    // Regra: devolver livro
    public void Return(BookCondition returnCondition)
    {
        if (ReturnAt != null)
            throw new LoanAlreadyReturnedException();

        ReturnAt = DateTime.UtcNow;
        ReturnCondition = returnCondition;
    }


    // Regra: verificar atraso
    public bool IsLate()
    {
        if (ReturnAt != null)
            return ReturnAt > GetDueDate();

        return DateTime.UtcNow > GetDueDate();
    }


    // Data limite de devolução
    public DateTime GetDueDate()
    {
        return StartAt.AddDays(Period);
    }
}
