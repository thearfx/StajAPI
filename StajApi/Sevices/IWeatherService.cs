using StajApi.Models;
using System.Threading.Tasks;

namespace StajApi.Services
{
    public interface IWeatherService
    {
        Task<WeatherResponse> GetCityWeatherAsync(string cityName);
    }
}