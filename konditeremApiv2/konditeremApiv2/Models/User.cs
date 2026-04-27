using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } = "user";
    
    public List<Rating>? Ratings { get; set; }

    public UserResponse GetResponse()
    {
        return new UserResponse
        {
            Id = Id,
            Name = Name,
            Email = Email,
            Role = Role,
            Ratings = Ratings
        };
    }
}