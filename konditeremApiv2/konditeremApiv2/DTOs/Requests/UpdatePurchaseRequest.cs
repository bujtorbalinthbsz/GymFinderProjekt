namespace konditeremApiv2.DTOs.Requests;

public class UpdatePurchaseRequest
{
    
    public Guid Id { get; set; }
    public required double Amount { get; set; } = 0.0;
    public DateTime? ExpirationDate { get; set; } = null;
    public required int ProductId { get; set; }
    public required int UserId { get; set; }
}