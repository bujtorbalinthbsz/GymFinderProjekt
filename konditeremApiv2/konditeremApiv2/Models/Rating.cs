using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Models;

public class Rating
{
    public int Id { get; set; }
    public int Stars { get; set; }
    public string? Message { get; set; } = null;
    public int UserId { get; set; }
    public int GymId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    [ForeignKey("GymId")]
    public Gym Gym { get; set; }

    public RatingResponse GetResponse()
    {
        return new RatingResponse
        {
            Id = Id,
            Stars = Stars,
            Message = Message,
            User = User,
            Gym = Gym
        };
    }
}