using Library.Domain.Enums;
using Library.Domain.Exceptions;

namespace Library.Domain.Entities;

public class Book
{
    public string Isbn { get; private set; }
    public string Title { get; private set; }
    public int ReleaseYear { get; private set; }
    public string Author { get; private set; }
    public List<BookGenre> Genres { get; private set; }
    public string? Summary { get; private set; }
    public int? PageLength { get; private set; }
    public string? Publisher { get; private set; }


    // construtor c/ validações
    public Book(string isbn,
                string title,
                int releaseYear,
                string author,
                List<BookGenre> genres,
                string? summary = null,
                int? pageLength = null,
                string? publisher = null)

    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN is required");

        if (isbn.Length != 13)
            throw new InvalidIsbnException("Invalid ISBN");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required");

        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author is required");

        if (releaseYear < 1000 || releaseYear > DateTime.Now.Year)
            throw new ArgumentException("Invalid release year");

        if (genres == null || genres.Count == 0)
            throw new ArgumentException("At least one genre is required");


        Isbn = isbn;
        Title = title;
        ReleaseYear = releaseYear;
        Author = author;
        Genres = genres;
        Summary = summary;
        PageLength = pageLength;
        Publisher = publisher;
    }

    public void UpdateSummary(string summary) // metodo de compartamento
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary is invalid");

        Summary = summary;
    }

    public void ChangePublisher(string publisher)
    {
        if (string.IsNullOrWhiteSpace(publisher))
            throw new ArgumentException("Publisher is invalid");

        Publisher = publisher;
    }


}

//private set - só o proprio obejto pode mudar
// ? - campos podem ser nulos