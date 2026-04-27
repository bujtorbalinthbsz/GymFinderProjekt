using System.ComponentModel.DataAnnotations.Schema;

namespace konditeremApiv2.Models;

public class GymHasProduct
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public int ProductId { get; set; }
    
    [ForeignKey("GymId")]
    public Gym Gym { get; set; }
    
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}