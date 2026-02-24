using Library.Domain.Entities;

namespace Library.Domain.Interfaces;

public interface IBookService
{
    void Create(Book book);
    Book? GetByIsbn(string isbn);
    IEnumerable<Book> GetAll();
    void Update(Book book);
    void Delete(string isbn);

}
























