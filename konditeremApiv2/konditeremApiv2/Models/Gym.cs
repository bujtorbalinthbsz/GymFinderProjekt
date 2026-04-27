using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Models;

public class Gym
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; } = null;
    public string? Email { get; set; } = null;
    public int? CityId { get; set; }
    
    [ForeignKey("CityId")]
    public City? City { get; set; }

    public required string OpenAt { get; set; } = "{}";
    public string? ImagePath { get; set; } = null;
    
    public List<Rating>? Ratings { get; set; }
    
    public List<GymHasProduct>? Products { get; set; }

    public GymResponse GetResponse()
    {
        JsonObject parsedOpenAt;
        try
        {
            // Megpróbáljuk szabályos JSON-ként értelmezni
            parsedOpenAt = string.IsNullOrWhiteSpace(OpenAt) 
                ? new JsonObject() 
                : JsonNode.Parse(OpenAt)?.AsObject() ?? new JsonObject();
        }
        catch
        {
            // Ha elhasal (mert pl. "Hétfő..." van beírva), becsomagoljuk egy JSON-ba, hogy ne fagyjon ki!
            parsedOpenAt = new JsonObject { ["info"] = OpenAt };
        }

        return new GymResponse
        {
            Id = Id,
            Name = Name,
            Phone = Phone,
            Email = Email,
            City = City,
            OpenAt = parsedOpenAt,
            ImagePath = ImagePath,
            Ratings = Ratings,
            Products = Products,
        };
    }
}