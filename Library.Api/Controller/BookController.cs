using Microsoft.AspNetCore.Mvc;
using Library.Domain.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Application.DTO;


namespace Library.API.Controllers;

[ApiController]
[Route("books")]
public class BookController : ControllerBase
{
    private readonly IBookService _service;

    public BookController(IBookService service)
    {
        _service = service;
    }
    [HttpPost]
    public IActionResult Create([FromBody] CreateBookRequest request)
    {
           var genres = request.Genres
            .Select(g => Enum.Parse<BookGenre>(g, true))
            .ToList();

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
        

        _service.Create(book);

        return Ok(new ApiResponse<string>
        {
            Message = "Livro criado com sucesso.",
            Timestamp = DateTime.UtcNow,
            Data = request.Isbn
        });
    }
    [HttpGet]
    public IActionResult GetAll()
    {
        var books = _service.GetAll();

        var response = books.Select(b => new BookResponse
        {
            Isbn = b.Isbn,
            Title = b.Title,
            ReleaseYear = b.ReleaseYear,
            Summary = b.Summary,
            Author = b.Author,
            PageLength = b.PageLength,
            Publisher = b.Publisher,
            Genres = b.Genres.Select(g => g.ToString()).ToList()
        });

        return Ok(new ApiResponse<IEnumerable<BookResponse>>
        {
            Message = "Livros encontrados.",
            Timestamp = DateTime.UtcNow,
            Data = response
        });
    }

    [HttpGet("{isbn}")]
    public IActionResult GetByIsbn(string isbn)
    {
        var book = _service.GetByIsbn(isbn);

        if (book == null)
            return NotFound(new ApiResponse<string>
            {
                Message = "Livro não encontrado.",
                Timestamp = DateTime.UtcNow
            });

        var response = new BookResponse
        {
            Isbn = book.Isbn,
            Title = book.Title,
            ReleaseYear = book.ReleaseYear,
            Summary = book.Summary,
            Author = book.Author,
            PageLength = book.PageLength,
            Publisher = book.Publisher,
            Genres = book.Genres.Select(g => g.ToString()).ToList()
        };

        return Ok(new ApiResponse<BookResponse>
        {
            Message = "Livro encontrado.",
            Timestamp = DateTime.UtcNow,
            Data = response
        });
    }

    [HttpPut("{isbn}")]
    public IActionResult Update(string isbn, [FromBody] UpdateBookRequest request)
    {
        var genres = request.Genres
            .Select(g => Enum.Parse<BookGenre>(g, true))
            .ToList();

        var book = new Book(
            isbn,
            request.Title,
            request.ReleaseYear,
            request.Author,
            genres,
            request.Summary,
            request.PageLength,
            request.Publisher
        );

        _service.Update(book);

        return Ok(new ApiResponse<string>
        {
            Message = "Livro atualizado com sucesso.",
            Timestamp = DateTime.UtcNow,
            Data = isbn
        });
    }


    [HttpDelete("{isbn}")]
    public IActionResult Delete(string isbn)
    {
        _service.Delete(isbn);

        return Ok(new ApiResponse<string>
        {
            Message = "Livro excluído com sucesso.",
            Timestamp = DateTime.UtcNow,
            Data = isbn
        });
    }
}