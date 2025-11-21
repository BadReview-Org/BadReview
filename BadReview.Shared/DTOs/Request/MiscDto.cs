namespace BadReview.Shared.DTOs.Request;

public interface ICountriesIso {
    Task<IsoCountry?> GetAsync(int code);
    Task<Dictionary<int, IsoCountry>> GetAsync();
    Task<string> GetCountryName(int? countryCode);
};

public record IsoCountry(string Name, string Alpha_3, int Country_code);