using Library.Domain.Entities;
using NUlid;

namespace Library.Domain.Repositories;

public interface ILoanRepository
{
    void Create(Loan loan);
    public void Delete (Ulid id);
    Loan? GetById(Ulid id);
    IEnumerable<Loan> GetAll();
    IEnumerable<Loan> GetByUserId(Ulid userId);
    void UpdateReturn(Loan loan);
}
