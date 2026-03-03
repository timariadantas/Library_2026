using Library.Domain.Entities;
using Library.Domain.Repositories;
using Library.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Library.Infrastructure.Postgres;

public class PostgresBookRepository : IBookRepository
{
    private readonly string _connectionString;

    public PostgresBookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new ArgumentNullException("Connection string not found.");
    }

    private NpgsqlConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);

    public void Create(Book book)
    {
        using var conn = CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            var bookSql = """
                INSERT INTO books
                (isbn, title, release_year, summary, author, page_len, publisher)
                VALUES
                (@isbn, @title, @releaseYear, @summary, @author, @pageLen, @publisher);
            """;

            using var bookCmd = new NpgsqlCommand(bookSql, conn, transaction);

            bookCmd.Parameters.AddWithValue("isbn", book.Isbn);
            bookCmd.Parameters.AddWithValue("title", book.Title);
            bookCmd.Parameters.AddWithValue("releaseYear", book.ReleaseYear);
            bookCmd.Parameters.AddWithValue("summary", (object?)book.Summary ?? DBNull.Value);
            bookCmd.Parameters.AddWithValue("author", book.Author);
            bookCmd.Parameters.AddWithValue("pageLen", (object?)book.PageLength ?? DBNull.Value);
            bookCmd.Parameters.AddWithValue("publisher", (object?)book.Publisher ?? DBNull.Value);

            bookCmd.ExecuteNonQuery();

            var genreSql = """
                INSERT INTO genre (book_id, genre)
                VALUES (@bookId, @genre::book_genre);
            """;

            foreach (var genre in book.Genres)
            {
                using var genreCmd = new NpgsqlCommand(genreSql, conn, transaction);
                genreCmd.Parameters.AddWithValue("bookId", book.Isbn);
                genreCmd.Parameters.AddWithValue("genre", genre.ToString().ToLower());
                genreCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Book? GetByIsbn(string isbn)
    {
        using var conn = CreateConnection();
        conn.Open();

        var bookSql = """
            SELECT isbn, title, release_year, summary, author, page_len, publisher
            FROM books
            WHERE isbn = @isbn;
        """;

        using var bookCmd = new NpgsqlCommand(bookSql, conn);
        bookCmd.Parameters.AddWithValue("isbn", isbn);

        using var reader = bookCmd.ExecuteReader();

        if (!reader.Read())
            return null;

        var title = reader.GetString(reader.GetOrdinal("title"));
        var releaseYear = reader.GetInt32(reader.GetOrdinal("release_year"));
        var author = reader.GetString(reader.GetOrdinal("author"));

        var summary = reader.IsDBNull(reader.GetOrdinal("summary"))
            ? null
            : reader.GetString(reader.GetOrdinal("summary"));

        int? pageLength = reader.IsDBNull(reader.GetOrdinal("page_len"))
            ? null
            : reader.GetInt32(reader.GetOrdinal("page_len"));

        var publisher = reader.IsDBNull(reader.GetOrdinal("publisher"))
            ? null
            : reader.GetString(reader.GetOrdinal("publisher"));

        reader.Close();

        var genreSql = """
            SELECT genre
            FROM genre
            WHERE book_id = @isbn;
        """;

        using var genreCmd = new NpgsqlCommand(genreSql, conn);
        genreCmd.Parameters.AddWithValue("isbn", isbn);

        using var genreReader = genreCmd.ExecuteReader();

        var genres = new List<BookGenre>();

        while (genreReader.Read())
        {
            var genreString = genreReader.GetString(0);
            var parsedGenre = Enum.Parse<BookGenre>(genreString, true);
            genres.Add(parsedGenre);
        }

        return new Book(
            isbn,
            title,
            releaseYear,
            author,
            genres,
            summary,
            pageLength,
            publisher
        );
    }

    public void Update(Book book)
    {
        using var conn = CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            var updateBookSql = """
                UPDATE books
                SET title = @title,
                    release_year = @releaseYear,
                    summary = @summary,
                    author = @author,
                    page_len = @pageLen,
                    publisher = @publisher
                WHERE isbn = @isbn;
            """;

            using var updateBookCmd = new NpgsqlCommand(updateBookSql, conn, transaction);

            updateBookCmd.Parameters.AddWithValue("isbn", book.Isbn);
            updateBookCmd.Parameters.AddWithValue("title", book.Title);
            updateBookCmd.Parameters.AddWithValue("releaseYear", book.ReleaseYear);
            updateBookCmd.Parameters.AddWithValue("summary", (object?)book.Summary ?? DBNull.Value);
            updateBookCmd.Parameters.AddWithValue("author", book.Author);
            updateBookCmd.Parameters.AddWithValue("pageLen", (object?)book.PageLength ?? DBNull.Value);
            updateBookCmd.Parameters.AddWithValue("publisher", (object?)book.Publisher ?? DBNull.Value);

            updateBookCmd.ExecuteNonQuery();

            var deleteGenresSql = "DELETE FROM genre WHERE book_id = @bookId;";
            using var deleteCmd = new NpgsqlCommand(deleteGenresSql, conn, transaction);
            deleteCmd.Parameters.AddWithValue("bookId", book.Isbn);
            deleteCmd.ExecuteNonQuery();

            var insertGenreSql = """
                INSERT INTO genre (book_id, genre)
                VALUES (@bookId, @genre::book_genre);
            """;

            foreach (var genre in book.Genres)
            {
                using var insertCmd = new NpgsqlCommand(insertGenreSql, conn, transaction);
                insertCmd.Parameters.AddWithValue("bookId", book.Isbn);
                insertCmd.Parameters.AddWithValue("genre", genre.ToString().ToLower());
                insertCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Delete(string isbn)
    {
        using var conn = CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            using var deleteGenres = new NpgsqlCommand(
                "DELETE FROM genre WHERE book_id = @isbn;",
                conn, transaction);

            deleteGenres.Parameters.AddWithValue("isbn", isbn);
            deleteGenres.ExecuteNonQuery();

            using var deleteBook = new NpgsqlCommand(
                "DELETE FROM books WHERE isbn = @isbn;",
                conn, transaction);

            deleteBook.Parameters.AddWithValue("isbn", isbn);
            deleteBook.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public bool ExistsActiveLoan(string isbn)
    {
        using var conn = CreateConnection();
        conn.Open();

        var sql = """
            SELECT 1
            FROM loan l
            JOIN portfolio p ON p.id = l.portfolio_id
            WHERE p.book_id = @isbn
            AND l.return_at IS NULL;
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("isbn", isbn);

        return cmd.ExecuteScalar() != null;
    }

    public IEnumerable<Book> GetAll()
{
    var books = new List<Book>();

    using var conn = CreateConnection();
    conn.Open();

    var bookSql = """
        SELECT isbn, title, release_year, summary, author, page_len, publisher
        FROM books;
    """;

    using var bookCmd = new NpgsqlCommand(bookSql, conn);
    using var reader = bookCmd.ExecuteReader();

    var bookData = new List<(string isbn, string title, int releaseYear, string author, string? summary, int? pageLen, string? publisher)>();

    while (reader.Read())
    {
        var isbn = reader.GetString(reader.GetOrdinal("isbn"));
        var title = reader.GetString(reader.GetOrdinal("title"));
        var releaseYear = reader.GetInt32(reader.GetOrdinal("release_year"));
        var author = reader.GetString(reader.GetOrdinal("author"));

        var summary = reader.IsDBNull(reader.GetOrdinal("summary"))
            ? null
            : reader.GetString(reader.GetOrdinal("summary"));

        int? pageLen = reader.IsDBNull(reader.GetOrdinal("page_len"))
            ? null
            : reader.GetInt32(reader.GetOrdinal("page_len"));

        var publisher = reader.IsDBNull(reader.GetOrdinal("publisher"))
            ? null
            : reader.GetString(reader.GetOrdinal("publisher"));

        bookData.Add((isbn, title, releaseYear, author, summary, pageLen, publisher));
    }

    reader.Close();

    foreach (var data in bookData)
    {
        var genreSql = """
            SELECT genre
            FROM genre
            WHERE book_id = @isbn;
        """;

        using var genreCmd = new NpgsqlCommand(genreSql, conn);
        genreCmd.Parameters.AddWithValue("isbn", data.isbn);

        using var genreReader = genreCmd.ExecuteReader();

        var genres = new List<BookGenre>();

        while (genreReader.Read())
        {
            var genreString = genreReader.GetString(0);
            var parsedGenre = Enum.Parse<BookGenre>(genreString, true);
            genres.Add(parsedGenre);
        }

        books.Add(new Book(
            data.isbn,
            data.title,
            data.releaseYear,
            data.author,
            genres,
            data.summary,
            data.pageLen,
            data.publisher
        ));
    }

    return books;
}

public IEnumerable<Book> GetByAuthor(string author)
{
    var books = new List<Book>();

    using var conn = CreateConnection();
    conn.Open();

    var sql = """
        SELECT isbn, title, release_year, summary, author, page_len, publisher
        FROM books
        WHERE author = @author;
    """;

    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("author", author);

    using var reader = cmd.ExecuteReader();

    var bookData = new List<(string isbn, string title, int releaseYear, string? summary, int? pageLength, string? publisher)>();

    while (reader.Read())
    {
        var isbn = reader.GetString(reader.GetOrdinal("isbn"));
        var title = reader.GetString(reader.GetOrdinal("title"));
        var releaseYear = reader.GetInt32(reader.GetOrdinal("release_year"));

        var summary = reader.IsDBNull(reader.GetOrdinal("summary"))
            ? null
            : reader.GetString(reader.GetOrdinal("summary"));

        int? pageLength = reader.IsDBNull(reader.GetOrdinal("page_len"))
            ? null
            : reader.GetInt32(reader.GetOrdinal("page_len"));

        var publisher = reader.IsDBNull(reader.GetOrdinal("publisher"))
            ? null
            : reader.GetString(reader.GetOrdinal("publisher"));

        bookData.Add((isbn, title, releaseYear, summary, pageLength, publisher));
    }

    reader.Close();

    foreach (var data in bookData)
    {
        var genreSql = """
            SELECT genre
            FROM genre
            WHERE book_id = @isbn;
        """;

        using var genreCmd = new NpgsqlCommand(genreSql, conn);
        genreCmd.Parameters.AddWithValue("isbn", data.isbn);

        using var genreReader = genreCmd.ExecuteReader();

        var genres = new List<BookGenre>();

        while (genreReader.Read())
        {
            var genreString = genreReader.GetString(0);
            var parsedGenre = Enum.Parse<BookGenre>(genreString, true);
            genres.Add(parsedGenre);
        }

        books.Add(new Book(
            data.isbn,
            data.title,
            data.releaseYear,
            author,
            genres,
            data.summary,
            data.pageLength,
            data.publisher
        ));
    }

    return books;
}

    
 
}