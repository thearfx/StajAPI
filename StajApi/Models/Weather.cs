// StajApi/Models/Weather.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StajApi.Core.Entities.Enums;
using System.Collections.Generic;

namespace StajApi.Models
{
    // API yanıtının tamamını temsil eden ana sınıf
    public class WeatherResponse
    {
        [JsonProperty("name")]
        public string City { get; set; }

        [JsonProperty("main")]
        public MainInfo Main { get; set; }

        [JsonProperty("weather")]
        public List<WeatherDescription> Weather { get; set; }

        [JsonProperty("cod")]
        public int Cod { get; set; } // HTTP durumu (200 OK, 404 vb.)
    }

    // Sıcaklık, nem gibi ana bilgileri içeren sınıf
    public class MainInfo
    {
        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }

        [JsonProperty("temp_min")]
        public double TempMin { get; set; }

        [JsonProperty("temp_max")]
        public double TempMax { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }
    }

    // Hava durumunun açıklamalarını içeren sınıf
    public class WeatherDescription
    {
        [JsonProperty("main")]
        [JsonConverter(typeof(StringEnumConverter))] // JSON'daki string'i enum'a dönüştürmek için
        public WeatherCondition Main { get; set; } // string yerine enum kullanıyoruz

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}