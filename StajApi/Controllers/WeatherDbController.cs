using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajApi.Core.Infrastructure.Utilities;
using StajApi.Data;
using StajApi.Entities;
using StajApi.Entities.Enums;
using StajApi.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StajApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherDbController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWeatherService _weatherService;
        private readonly IConfiguration _configuration;

        private readonly List<string> _turkishCities = new List<string>
        {
            "Istanbul", "Ankara", "Izmir", "Bursa", "Antalya", "Adana", "Gaziantep", "Konya", "Eskisehir"
        };

        public WeatherDbController(ApplicationDbContext dbContext, IWeatherService weatherService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _weatherService = weatherService;
            _configuration = configuration;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoarResponse<List<WeatherEntry>>), StatusCodes.Status200OK)]
        public async Task<RoarResponse<List<WeatherEntry>>> GetWeatherFromDb([FromQuery] string? city = null)
        {
            IQueryable<WeatherEntry> query = _dbContext.WeatherEntries;

            if (!string.IsNullOrEmpty(city))
            {
                // Eğer şehir parametresi varsa, sorguyu filtrele
                query = query.Where(e => e.City.ToLower() == city.ToLower());
            }

            var weatherEntries = await query.ToListAsync();

            if (!weatherEntries.Any())
            {
                if (!string.IsNullOrEmpty(city))
                {
                    return new RoarResponse<List<WeatherEntry>>(false, $"{city} şehrine ait hava durumu kaydı bulunamadı.", null)
                    {
                        ResponseType = RoarResponseCodeType.Error,
                        HttpStatusCode = StatusCodes.Status404NotFound
                    };
                }
                else
                {
                    return new RoarResponse<List<WeatherEntry>>(false, "Veritabanında hava durumu kaydı bulunamadı.", null)
                    {
                        ResponseType = RoarResponseCodeType.Error,
                        HttpStatusCode = StatusCodes.Status404NotFound
                    };
                }
            }

            return new RoarResponse<List<WeatherEntry>>(true, "Hava durumu verileri başarıyla çekildi.", weatherEntries)
            {
                ResponseType = RoarResponseCodeType.Success,
                HttpStatusCode = StatusCodes.Status200OK,
                TotalCount = weatherEntries.Count
            };
        }

        [HttpPost("save")]
        [ProducesResponseType(typeof(RoarResponse<List<WeatherEntry>>), StatusCodes.Status200OK)]
        public async Task<RoarResponse<List<WeatherEntry>>> SaveWeatherToDb([FromQuery] List<string>? name = null)
        {
            var citiesToFetch = new List<string>();

            if (name != null && name.Any())
            {
                var districtsSection = _configuration.GetSection("Districts");
                var districts = districtsSection.Get<Dictionary<string, List<string>>>();

                foreach (var cityOrDistrict in name)
                {
                    if (districts != null && districts.ContainsKey(cityOrDistrict))
                    {
                        citiesToFetch.Add(cityOrDistrict);
                        citiesToFetch.AddRange(districts[cityOrDistrict]);
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

            var weatherList = new List<WeatherEntry>();

            foreach (var city in citiesToFetch)
            {
                var weatherData = await _weatherService.GetCityWeatherAsync(city);
                if (weatherData != null && weatherData.Cod == 200)
                {
                    var weatherEntry = new WeatherEntry
                    {
                        City = weatherData.City,
                        Temperature = weatherData.Main.Temp,
                        FeelsLike = weatherData.Main.FeelsLike,
                        TempMin = weatherData.Main.TempMin,
                        TempMax = weatherData.Main.TempMax,
                        Humidity = weatherData.Main.Humidity,
                        Description = weatherData.Weather.FirstOrDefault()?.Description,
                        Timestamp = DateTime.Now
                    };
                    weatherList.Add(weatherEntry);
                }
            }

            _dbContext.WeatherEntries.AddRange(weatherList);
            await _dbContext.SaveChangesAsync();

            return new RoarResponse<List<WeatherEntry>>(true, "Hava durumu verileri başarıyla veritabanına kaydedildi.", weatherList)
            {
                ResponseType = RoarResponseCodeType.Success,
                HttpStatusCode = StatusCodes.Status201Created,
                TotalCount = weatherList.Count
            };
        }
    }
}