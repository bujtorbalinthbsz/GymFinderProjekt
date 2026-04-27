namespace konditeremApiv2.DTOs.Requests;

public class CreateRatingRequest
{
    public required int Stars { get; set; }
    public string? Message { get; set; } = null;
    public required int UserId { get; set; }
    public required int GymId { get; set; }
}