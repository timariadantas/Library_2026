using Library.Domain.Repositories;
using Library.Domain.Interfaces;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _repository;
    private readonly ILoanRepository _loanRepository;

    public BookService(IBookRepository repository, ILoanRepository loanRepository)
    {
        _repository = repository;
        _loanRepository = loanRepository;

    }

    public void Create(Book book)
    {
        // Regra de negócio 
        var existing = _repository.GetByIsbn(book.Isbn);
        if (existing != null)
            throw new Exception("Livro já cadastrado.");

        _repository.Create(book);
    }

    public Book? GetByIsbn(string isbn)
    {
        return _repository.GetByIsbn(isbn);
    }

    public IEnumerable<Book> GetAll()
    {
        return _repository.GetAll();
    }

    public void Update(Book book)
    {
        var existing = _repository.GetByIsbn(book.Isbn);
        if (existing == null)
            throw new Exception("Livro não encontrado.");

        _repository.Update(book);
    }

    public void Delete(string isbn)
    {
        var book = _repository.GetByIsbn(isbn);
        if (book == null)
            throw new Exception("Livro não encontrado.");

        var isBorrowed = _loanRepository.ExistsActiveLoan(isbn);

        if (isBorrowed)
            throw new Exception("Livro está emprestado e não pode ser excluído.");

        _repository.Delete(isbn);
    }

    
}

    
