using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Domain.Repositories;

public interface IBookRepository
{
  void Create(Book book);
  Book? GetByIsbn(string isbn);
  List<Book> All { get; }
  void Update(Book book);
  void Delete(string isbn);
  IEnumerable<Book> GetAll();
  bool Exists(string isbn);
  IEnumerable<Book> GetByAuthor(string author);

  //                                                                                               bool Exists(string isbn);
}
