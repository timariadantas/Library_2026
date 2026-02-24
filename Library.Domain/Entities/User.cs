using NUlid;
using Library.Domain.Exceptions;

namespace Library.Domain.Entities;

public class User
{
    public Ulid Id { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? InactivatedAt { get; private set; }

    public bool Active { get; private set; }


    // Construtor principal (criação do usuário)
    public User(string name, string email)
    {
        Validate(name, email);

        Id = Ulid.NewUlid();
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        Active = true;
    }


    // Validações do domínio
    private void Validate(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidUserNameException("name is required.");

        if (name.Length > 256)
            throw new InvalidUserNameException("Name is too long");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException();

        if (!email.Contains("@"))
            throw new InvalidEmailException();
    }


    // Regra de negócio: desativar usuário
    public void Disabled()
    {
        if (!Active)
            throw new UserAlreadyInactiveException();

        Active = false;
        InactivatedAt = DateTime.UtcNow;
    }


    // Regra de negócio: reativar usuário
    public void Activate()
    {
        if (Active)
            throw new UserAlreadyActiveException();

        Active = true;
        InactivatedAt = null;
    }
}
