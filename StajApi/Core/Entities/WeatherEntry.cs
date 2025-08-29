// StajApi/Entities/WeatherEntry.cs

namespace StajApi.Entities
{
    public class WeatherEntry
    {
        public int Id { get; set; }
        public string City { get; set; }
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public int Humidity { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } // Verinin ne zaman kaydedildiğini gösterir
    }
}