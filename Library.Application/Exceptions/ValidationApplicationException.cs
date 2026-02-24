namespace Library.Application.Exceptions;

public class ValidationApplicationException : Exception
{
        public ValidationApplicationException(string message)
        : base(message)
    {
    }

}

// não confundir com FluentValidation