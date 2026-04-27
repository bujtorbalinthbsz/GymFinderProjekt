using konditeremApiv2.Models;

namespace konditeremApiv2.DTOs.Requests;

public class CreatePurchaseRequest
{
    public required bool IsCash { get; set; } = false;
    public required bool IsCreditCard { get; set; } = false;
    public required double Amount { get; set; } = 0.0;
    public DateTime? ExpirationDate { get; set; } = null;
    public required int ProductId { get; set; }
    public required int UserId { get; set; }
    public required int GymId { get; set; }
}