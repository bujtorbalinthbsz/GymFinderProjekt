using System.ComponentModel.DataAnnotations.Schema;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Models;

public class Product
{
    public int Id { get; set; }
    public string Designation { get; set; }
    public bool IsTicket { get; set; }
    public bool IsRental { get; set; }
    public bool IsActive { get; set; }
    public double Price { get; set; } = 0.0;
    
    public List<GymHasProduct>? Gyms { get; set; }

    public ProductResponse GetResponse()
    {
        return new ProductResponse
        {
            Id = Id,
            Designation = Designation,
            IsTicket = IsTicket,
            IsRental = IsRental,
            IsActive = IsActive,
            Price = Price,
            Gyms = Gyms
        };
    }
}