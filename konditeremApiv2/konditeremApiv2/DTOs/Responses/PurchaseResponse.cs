using konditeremApiv2.Models;

namespace konditeremApiv2.DTOs.Responses;

public class PurchaseResponse
{
    public Guid Id { get; set; }
    public bool IsCash { get; set; } = false;
    public bool IsCreditCard { get; set; } = false;
    public double Amount { get; set; }
    public DateTime? ExpirationDate { get; set; } = null;
    public Product Product { get; set; }
    public User User { get; set; }
    public User Cashier { get; set; }
    public Gym Gym { get; set; }
}