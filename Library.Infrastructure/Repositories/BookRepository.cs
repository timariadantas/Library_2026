using Library.Domain.Entities;
using Library.Domain.Repositories;
using Library.Domain.Enums;
using Npgsql;
using System.Data;

namespace Library.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }


    public void Create(Book book)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            // 1️⃣ Inserir livro
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

            // Inserir gêneros (enum PostgreSQL)
            var genreSql = """
            INSERT INTO genre (book_id, genre)
            VALUES (@bookId, @genre::book_genre);
        """;

            foreach (var genre in book.Genres)
            {
                using var genreCmd = new NpgsqlCommand(genreSql, conn, transaction);

                genreCmd.Parameters.AddWithValue("bookId", book.Isbn);

                // converter enum C# para string para PostgreSQL
                genreCmd.Parameters.AddWithValue(
                    "genre",
                    genre.ToString().ToLower()
                );

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
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        // Buscar livro
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

        string? summary = null;
        if (!reader.IsDBNull(reader.GetOrdinal("summary")))
            summary = reader.GetString(reader.GetOrdinal("summary"));

        int? pageLength = null;
        if (!reader.IsDBNull(reader.GetOrdinal("page_len")))
            pageLength = reader.GetInt32(reader.GetOrdinal("page_len"));

        string? publisher = null;
        if (!reader.IsDBNull(reader.GetOrdinal("publisher")))
            publisher = reader.GetString(reader.GetOrdinal("publisher"));

        reader.Close();

        // Buscar gêneros (ENUM PostgreSQL → string)
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

            var parsedGenre = Enum.Parse<BookGenre>(
                genreString,
                ignoreCase: true
            );

            genres.Add(parsedGenre);
        }

        // Criar entidade
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
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            // Atualizar os dados principais do livro
            var updateBookSql = """
            UPDATE books
            SET
                title = @title,
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

            //  Remover gêneros antigos
            var deleteGenresSql = """
            DELETE FROM genre
            WHERE book_id = @bookId;
        """;

            using var deleteGenresCmd = new NpgsqlCommand(deleteGenresSql, conn, transaction);
            deleteGenresCmd.Parameters.AddWithValue("bookId", book.Isbn);
            deleteGenresCmd.ExecuteNonQuery();

            // novos gêneros
            var insertGenreSql = """
            INSERT INTO genre (book_id, genre)
            VALUES (@bookId, @genre);
        """;

            foreach (var genre in book.Genres)
            {
                using var insertGenreCmd = new NpgsqlCommand(insertGenreSql, conn, transaction);

                insertGenreCmd.Parameters.AddWithValue("bookId", book.Isbn);
                insertGenreCmd.Parameters.AddWithValue(
                    "genre",
                    genre.ToString().ToLower()
                );

                insertGenreCmd.ExecuteNonQuery();
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
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            //  Remover gêneros primeiro (FK)
            var deleteGenresSql = """
            DELETE FROM book_genres
            WHERE isbn = @isbn;
        """;

            using var deleteGenresCmd = new NpgsqlCommand(deleteGenresSql, conn, transaction);
            deleteGenresCmd.Parameters.AddWithValue("isbn", isbn);
            deleteGenresCmd.ExecuteNonQuery();

            // Remover livro
            var deleteBookSql = """
            DELETE FROM books
            WHERE isbn = @isbn;
        """;

            using var deleteBookCmd = new NpgsqlCommand(deleteBookSql, conn, transaction);
            deleteBookCmd.Parameters.AddWithValue("isbn", isbn);
            deleteBookCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public IEnumerable<Book> All
    {
        get
        {
            var books = new List<Book>();

            using var conn = _connectionFactory.CreateConnection();
            conn.Open();

            var sql = """
        SELECT isbn, title, release_year, summary, author, page_len, publisher
        FROM books;
    """;

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var isbn = reader.GetString(reader.GetOrdinal("isbn"));
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

                // Buscar gêneros do livro
                var genreSql = """
            SELECT genre_id
            FROM book_genres
            WHERE isbn = @isbn;
        """;

                using var genreCmd = new NpgsqlCommand(genreSql, conn);
                genreCmd.Parameters.AddWithValue("isbn", isbn);

                using var genreReader = genreCmd.ExecuteReader();

                var genres = new List<BookGenre>();

                while (genreReader.Read())
                {
                    var genreId = genreReader.GetInt32(0);
                    genres.Add((BookGenre)genreId);
                }

                books.Add(new Book(
                    isbn,
                    title,
                    releaseYear,
                    author,
                    genres,
                    summary,
                    pageLength,
                    publisher
                ));
            }

            return books;
        }
    }

    List<Book> IBookRepository.All => throw new NotImplementedException();

    // Verificar se existe 
    public bool Exists(string isbn)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        var sql = """
        SELECT 1
        FROM books
        WHERE isbn = @isbn;
    """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("isbn", isbn);

        return cmd.ExecuteScalar() != null;
    }

    // Buscar por autor

    public IEnumerable<Book> GetByAuthor(string author)
    {
        var books = new List<Book>();

        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        var sql = """
        SELECT isbn, title, release_year, summary, author, page_len, publisher
        FROM books
        WHERE author = @author;
    """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("author", author);

        using var reader = cmd.ExecuteReader();

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

            reader.Close();

            // buscar gêneros
            var genreSql = """
            SELECT genre_id
            FROM book_genres
            WHERE isbn = @isbn;
        """;

            using var genreCmd = new NpgsqlCommand(genreSql, conn);
            genreCmd.Parameters.AddWithValue("isbn", isbn);

            using var genreReader = genreCmd.ExecuteReader();

            var genres = new List<BookGenre>();

            while (genreReader.Read())
            {
                genres.Add((BookGenre)genreReader.GetInt32(0));
            }

            books.Add(new Book(
                isbn,
                title,
                releaseYear,
                author,
                genres,
                summary,
                pageLength,
                publisher
            ));
        }

        return books;
    }

    public IEnumerable<Book> GetAll()
    {
        throw new NotImplementedException();
    }
}



