namespace Library.Application.DTO;
public class CreateBookRequest
{
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string? Summary { get; set; }
    public string Author { get; set; } = string.Empty;
    public int? PageLength { get; set; }
    public string? Publisher { get; set; }

    // recebendo como string
    public List<string> Genres { get; set; } = new();
}