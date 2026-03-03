using Library.Domain.Entities;
using Library.Domain.Repositories;
using Library.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace Library.Infrastructure.Oracle;

public class OracleBookRepository : IBookRepository
{
    private readonly string _connectionString;

    public OracleBookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Oracle")
            ?? throw new ArgumentNullException("Connection string not found.");
    }

    private OracleConnection CreateConnection()
        => new OracleConnection(_connectionString);

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
                (:isbn, :title, :releaseYear, :summary, :author, :pageLen, :publisher)
            """;

            using var bookCmd = new OracleCommand(bookSql, conn);
            bookCmd.Transaction = transaction;

            bookCmd.Parameters.Add(new OracleParameter("isbn", book.Isbn));
            bookCmd.Parameters.Add(new OracleParameter("title", book.Title));
            bookCmd.Parameters.Add(new OracleParameter("releaseYear", book.ReleaseYear));
            bookCmd.Parameters.Add(new OracleParameter("summary", (object?)book.Summary ?? DBNull.Value));
            bookCmd.Parameters.Add(new OracleParameter("author", book.Author));
            bookCmd.Parameters.Add(new OracleParameter("pageLen", (object?)book.PageLength ?? DBNull.Value));
            bookCmd.Parameters.Add(new OracleParameter("publisher", (object?)book.Publisher ?? DBNull.Value));

            bookCmd.ExecuteNonQuery();

            var genreSql = """
                INSERT INTO genre (book_id, genre)
                VALUES (:bookId, :genre)
            """;

            foreach (var genre in book.Genres)
            {
                using var genreCmd = new OracleCommand(genreSql, conn);
                genreCmd.Transaction = transaction;

                genreCmd.Parameters.Add(new OracleParameter("bookId", book.Isbn));
                genreCmd.Parameters.Add(new OracleParameter("genre", genre.ToString().ToLower()));

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
            WHERE isbn = :isbn
        """;

        using var bookCmd = new OracleCommand(bookSql, conn);
        bookCmd.Parameters.Add(new OracleParameter("isbn", isbn));

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
            WHERE book_id = :isbn
        """;

        using var genreCmd = new OracleCommand(genreSql, conn);
        genreCmd.Parameters.Add(new OracleParameter("isbn", isbn));

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
                SET title = :title,
                    release_year = :releaseYear,
                    summary = :summary,
                    author = :author,
                    page_len = :pageLen,
                    publisher = :publisher
                WHERE isbn = :isbn
            """;

            using var updateBookCmd = new OracleCommand(updateBookSql, conn);
            updateBookCmd.Transaction = transaction;

            updateBookCmd.Parameters.Add(new OracleParameter("isbn", book.Isbn));
            updateBookCmd.Parameters.Add(new OracleParameter("title", book.Title));
            updateBookCmd.Parameters.Add(new OracleParameter("releaseYear", book.ReleaseYear));
            updateBookCmd.Parameters.Add(new OracleParameter("summary", (object?)book.Summary ?? DBNull.Value));
            updateBookCmd.Parameters.Add(new OracleParameter("author", book.Author));
            updateBookCmd.Parameters.Add(new OracleParameter("pageLen", (object?)book.PageLength ?? DBNull.Value));
            updateBookCmd.Parameters.Add(new OracleParameter("publisher", (object?)book.Publisher ?? DBNull.Value));

            updateBookCmd.ExecuteNonQuery();

            var deleteGenresSql = "DELETE FROM genre WHERE book_id = :bookId";

            using var deleteCmd = new OracleCommand(deleteGenresSql, conn);
            deleteCmd.Transaction = transaction;
            deleteCmd.Parameters.Add(new OracleParameter("bookId", book.Isbn));
            deleteCmd.ExecuteNonQuery();

            var insertGenreSql = """
                INSERT INTO genre (book_id, genre)
                VALUES (:bookId, :genre)
            """;

            foreach (var genre in book.Genres)
            {
                using var insertCmd = new OracleCommand(insertGenreSql, conn);
                insertCmd.Transaction = transaction;
                insertCmd.Parameters.Add(new OracleParameter("bookId", book.Isbn));
                insertCmd.Parameters.Add(new OracleParameter("genre", genre.ToString().ToLower()));
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
            using var deleteGenres = new OracleCommand(
                "DELETE FROM genre WHERE book_id = :isbn",
                conn);

            deleteGenres.Transaction = transaction;
            deleteGenres.Parameters.Add(new OracleParameter("isbn", isbn));
            deleteGenres.ExecuteNonQuery();

            using var deleteBook = new OracleCommand(
                "DELETE FROM books WHERE isbn = :isbn",
                conn);

            deleteBook.Transaction = transaction;
            deleteBook.Parameters.Add(new OracleParameter("isbn", isbn));
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
            WHERE p.book_id = :isbn
            AND l.return_at IS NULL
        """;

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("isbn", isbn));

        return cmd.ExecuteScalar() != null;
    }

    public IEnumerable<Book> GetAll()
    {
        var books = new List<Book>();

        using var conn = CreateConnection();
        conn.Open();

        var sql = """
            SELECT isbn, title, release_year, summary, author, page_len, publisher
            FROM books
        """;

        using var cmd = new OracleCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var isbn = reader.GetString(0);
            books.Add(GetByIsbn(isbn)!);
        }

        return books;
    }

    public IEnumerable<Book> GetByAuthor(string author)
    {
        var books = new List<Book>();

        using var conn = CreateConnection();
        conn.Open();

        var sql = """
            SELECT isbn
            FROM books
            WHERE author = :author
        """;

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("author", author));

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var isbn = reader.GetString(0);
            books.Add(GetByIsbn(isbn)!);
        }

        return books;
    }
}