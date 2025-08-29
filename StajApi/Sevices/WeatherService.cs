// StajApi/Services/WeatherService.cs
using StajApi.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging; // ILogger için

namespace StajApi.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<WeatherService> _logger; // Logger enjekte ettik

        public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WeatherApi:ApiKey"];
            _baseUrl = configuration["WeatherApi:BaseUrl"];
            _logger = logger;
        }
        
        public async Task<WeatherResponse> GetCityWeatherAsync(string cityName)
        {
            var url = $"{_baseUrl}?q={cityName},tr&appid={_apiKey}&units=metric&lang=tr";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(json);
                return weatherResponse;
            }
            else
            {
                _logger.LogError("API isteği başarısız oldu. Durum Kodu: {StatusCode}. Mesaj: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);

                // Hata durumunda, hata koduyla birlikte bir yanıt dönüyoruz
                return new WeatherResponse() { Cod = (int)response.StatusCode, City = cityName };
            }
        }
    }
}