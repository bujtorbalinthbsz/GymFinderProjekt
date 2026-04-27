using System.Text.Json.Nodes;
using konditeremApiv2.Models;

namespace konditeremApiv2.DTOs.Responses;

public class GymResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; } = null;
    public string? Email { get; set; } = null;
    public City? City { get; set; }
    public required JsonObject OpenAt { get; set; }
    public string? ImagePath { get; set; } = null;
    public List<Rating>? Ratings { get; set; }
    public List<GymHasProduct>? Products { get; set; }
}