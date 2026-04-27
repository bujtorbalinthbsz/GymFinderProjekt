using konditeremApiv2.Models;

namespace konditeremApiv2.DTOs.Responses;

public class CityResponse
{
    public int Id { get; set; }
    public int ZipCode { get; set; }
    public string Name { get; set; }
    public List<Gym>? Gyms { get; set; }
}