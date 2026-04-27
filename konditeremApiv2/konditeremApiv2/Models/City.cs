using System.ComponentModel.DataAnnotations.Schema;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Models;

public class City
{
    public int Id { get; set; }
    public int ZipCode { get; set; }
    public string Name { get; set; }
    
    public List<Gym>? Gyms { get; set; }

    public CityResponse GetResponse()
    {
        return new CityResponse
        {
            Id = Id,
            ZipCode = ZipCode,
            Name = Name,
            Gyms = Gyms
        };
    }
}