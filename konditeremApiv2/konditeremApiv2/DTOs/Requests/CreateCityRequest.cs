namespace konditeremApiv2.DTOs.Requests;

public class CreateCityRequest
{
    public required int ZipCode { get; set; }
    public required string Name { get; set; }
}