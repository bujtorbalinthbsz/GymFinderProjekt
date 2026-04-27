namespace konditeremApiv2.DTOs.Requests;

public class UpdateProductRequest
{
    public required int Id { get; set; }
    public required string Designation { get; set; }
    public bool IsTicket { get; set; } = false;
    public bool IsRental { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public double Price { get; set; } = 0.0;
    public List<int>? GymIds { get; set; } = null;
}