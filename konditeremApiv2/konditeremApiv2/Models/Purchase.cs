using System.ComponentModel.DataAnnotations.Schema;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Models;

public class Purchase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsCash { get; set; }
    public bool IsCreditCard { get; set; }
    public double Amount { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int CashierUserId { get; set; }
    public int GymId { get; set; }
    public DateTime? ExpirationDate { get; set; } = null;
    
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    [ForeignKey("CashierUserId")]
    public User Cashier { get; set; }

    [ForeignKey("GymId")]
    public Gym Gym { get; set; }

    public PurchaseResponse GetResponse()
    {
        return new PurchaseResponse
        {
            Id = Id,
            IsCash = IsCash,
            IsCreditCard = IsCreditCard,
            Amount = Amount,
            ExpirationDate = ExpirationDate,
            Product = Product,
            User = User,
            Cashier = Cashier,
            Gym = Gym
        };
    }
}