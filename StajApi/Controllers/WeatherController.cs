// StajApi/Controllers/WeatherController.cs
using Microsoft.AspNetCore.Mvc;
using StajApi.Core.Infrastructure.Utilities;
using StajApi.Entities.Enums;
using StajApi.Models;
using StajApi.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace StajApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IConfiguration _configuration;

        private readonly List<string> _turkishCities = new List<string>
        {
            "Istanbul", "Ankara", "Izmir", "Bursa", "Antalya", "Adana", "Gaziantep", "Konya", "Eskisehir"
        };

        public WeatherController(IWeatherService weatherService, IConfiguration configuration)
        {
            _weatherService = weatherService;
            _configuration = configuration;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoarResponse<List<WeatherResponse>>), StatusCodes.Status200OK)]
        public async Task<RoarResponse<List<WeatherResponse>>> GetWeather([FromQuery] List<string>? name = null)
        {
            var citiesToFetch = new List<string>();
            
            if (name != null && name.Any())
            {
                var districtsSection = _configuration.GetSection("Districts");
                var districts = districtsSection.Get<Dictionary<string, List<string>>>();

                foreach (var cityOrDistrict in name)
                {
                    var cityOrDistrictValidate = cityOrDistrict.ToLower();
                    if (districts != null && districts.ContainsKey(cityOrDistrictValidate))
                    {
                        citiesToFetch.Add(cityOrDistrictValidate);
                        citiesToFetch.AddRange(districts[cityOrDistrictValidate]);
                    }
                    else
                    {
                        citiesToFetch.Add(cityOrDistrict);
                    }
                }
            }
            else
            {
                citiesToFetch.AddRange(_turkishCities);
            }

            if (!citiesToFetch.Any())
            {
                return new RoarResponse<List<WeatherResponse>>(false, "Geçerli bir şehir veya ilçe bilgisi bulunamadı.", null)
                {
                    ResponseType = RoarResponseCodeType.Error,
                    HttpStatusCode = StatusCodes.Status400BadRequest
                };
            }

            var weatherList = new List<WeatherResponse>();
            
            foreach (var city in citiesToFetch)
            {
                var weatherData = await _weatherService.GetCityWeatherAsync(city);

                if (weatherData != null && weatherData.Cod == 401)
                {
                    return new RoarResponse<List<WeatherResponse>>(false, "API anahtarı (API Key) geçersiz veya aktif değil. Lütfen appsettings.json dosyasını kontrol edin.", null)
                    {
                        ResponseType = RoarResponseCodeType.Error,
                        HttpStatusCode = 401
                    };
                }
                
                if (weatherData != null && weatherData.Cod != 404)
                {
                    weatherList.Add(weatherData);
                }
            }
            
            
            if (!weatherList.Any())
            {
                return new RoarResponse<List<WeatherResponse>>(false, "Geçerli bir şehir veya ilçe bilgisi bulunamadı.", null)
                {
                    ResponseType = RoarResponseCodeType.Error,
                    HttpStatusCode = StatusCodes.Status404NotFound
                };
            }

            // Aksi halde başarılı yanıtı döndür.
            return new RoarResponse<List<WeatherResponse>>(true, "Hava durumu bilgisi başarıyla çekildi.", weatherList)
            {
                ResponseType = RoarResponseCodeType.Success,
                HttpStatusCode = StatusCodes.Status200OK,
                TotalCount = weatherList.Count
            };
        }
    }
}