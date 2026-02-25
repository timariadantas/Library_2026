using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Domain.Repositories;

public interface IBookRepository
{
  void Create(Book book);
  Book? GetByIsbn(string isbn);
  void Update(Book book);
  void Delete(string isbn);
  IEnumerable<Book> GetAll();
  bool ExistsActiveLoan(string isbn);
  IEnumerable<Book> GetByAuthor(string author);

  //                                                                                               bool Exists(string isbn);
}
