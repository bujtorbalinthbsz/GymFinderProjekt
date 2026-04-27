using konditeremApiv2.Models;

namespace konditeremApiv2.DTOs.Responses;

public class ProductResponse
{
    public int Id { get; set; }
    public string Designation { get; set; }
    public bool IsTicket { get; set; }
    public bool IsRental { get; set; }
    public bool IsActive { get; set; }
    public double Price { get; set; } = 0.0;
    
    public List<GymHasProduct>? Gyms { get; set; }
}