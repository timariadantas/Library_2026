namespace Library.Application.DTO;

public class ApiResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public T? Data { get; set; }

    public string? Error { get; set; }
}
