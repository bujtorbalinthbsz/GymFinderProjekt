namespace konditeremApiv2.DTOs.Requests;

public class UpdateCityRequest
{
    public int Id { get; set; }
    public required int ZipCode { get; set; }
    public required string Name { get; set; }
}