namespace konditeremApiv2.DTOs.Requests;

public class UpdateUserRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; } = null;
    public string Role { get; set; } = "user";
}