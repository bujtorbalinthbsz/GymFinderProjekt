namespace konditeremApiv2.DTOs.Requests;

public class UpdateRatingRequest
{
    public int Id { get; set; }
    public required int Stars { get; set; }
    public string? Message { get; set; } = null;
}