using Library.Domain.Repositories;
using Library.Domain.Interfaces;
using Library.Domain.Entities;
using Library.Application.DTO;
using Library.Application.Exceptions;
using Library.Application.Validators;
using Library.Domain.Enums;


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

    public void Create(CreateBookRequest request)
{
    // Validar
    var validator = new CreateBookRequestValidator();
    var validationResult = validator.Validate(request);

    if (!validationResult.IsValid)
        throw new ValidationApplicationException(
            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
        );
  // 2️⃣ Converter genres (string -> enum)
    var genres = request.Genres
        .Select(g => Enum.Parse<BookGenre>(g, ignoreCase: true))
        .ToList();
    // Regra de negócio
    var existing = _repository.GetByIsbn(request.Isbn);
    if (existing != null)
        throw new ConflictException("Livro já cadastrado.");

    // Criar entidade
    var book = new Book(
        request.Isbn,
        request.Title,
        request.ReleaseYear,
        request.Author,
        genres,
        request.Summary,
        request.PageLength,
        request.Publisher
        
        
    );

    // 4️⃣ Salvar
    _repository.Create(book);
}
 void IBookService.Create(Book book)
    {
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

    
