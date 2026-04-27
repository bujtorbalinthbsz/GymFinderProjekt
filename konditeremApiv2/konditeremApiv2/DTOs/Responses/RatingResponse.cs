using konditeremApiv2.Models;

namespace konditeremApiv2.DTOs.Responses;

public class RatingResponse
{
    public int Id { get; set; }
    public int Stars { get; set; }
    public string? Message { get; set; } = null;
    public required User User { get; set; }
    public required Gym Gym { get; set; }
}