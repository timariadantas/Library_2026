using NUlid;
using Library.Domain.Enums;
using Library.Domain.Exceptions;

namespace Library.Domain.Entities;

public class Portfolio
{
    public Ulid Id { get; private set; }

    public string BookId { get; private set; } // ISBN

    public BookCondition Condition { get; private set; }

    public BookCover Cover { get; private set; }


    public Portfolio(string bookId, BookCondition condition, BookCover cover)
    {
        Validate(bookId);

        Id = Ulid.NewUlid();
        BookId = bookId;
        Condition = condition;
        Cover = cover;
    }


    private void Validate(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
            throw new InvalidIsbnException("Invalid ISBN ");

        if (bookId.Length != 13)
            throw new InvalidIsbnException("Invalid ISBN");
    }


    // Regra: atualizar estado do livro
    public void UpdateCondition(BookCondition newCondition)
    {
        if (newCondition == BookCondition.Disable)
            throw new InvalidBookConditionException();

        Condition = newCondition;
    }
}
