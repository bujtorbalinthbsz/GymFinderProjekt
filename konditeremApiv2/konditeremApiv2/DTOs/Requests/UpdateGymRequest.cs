using System.Text.Json.Nodes;

namespace konditeremApiv2.DTOs.Requests;

public class UpdateGymRequest
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; } = null;
    public string? Email { get; set; } = null;
    public int? CityId { get; set; }
    public required JsonObject OpenAt { get; set; }
    public List<int>? ProductIds { get; set; } = null;
}