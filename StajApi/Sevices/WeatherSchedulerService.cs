using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StajApi.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using StajApi.Data;
using StajApi.Entities;

namespace StajApi.Services
{
    public class WeatherSchedulerService : BackgroundService
    {
        private readonly ILogger<WeatherSchedulerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly List<string> _turkishCities = new List<string>
        {
            "Istanbul", "Ankara", "Izmir", "Bursa", "Antalya", "Adana", "Gaziantep", "Konya", "Eskisehir"
        };

        public WeatherSchedulerService(ILogger<WeatherSchedulerService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hava durumu otomatik çekme servisi başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Hava durumu verisi çekiliyor...");

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var citiesToFetch = new List<string>();
                        var districts = _configuration.GetSection("Districts").Get<Dictionary<string, List<string>>>();

                        if (districts != null)
                        {
                            foreach (var city in districts.Keys)
                            {
                                citiesToFetch.Add(city);
                                citiesToFetch.AddRange(districts[city]);
                            }
                        }
                        citiesToFetch.AddRange(_turkishCities);

                        var weatherList = new List<WeatherResponse>();
                        foreach (var city in citiesToFetch)
                        {
                            var weatherData = await weatherService.GetCityWeatherAsync(city);
                            if (weatherData != null && weatherData.Cod != 404 && weatherData.Cod != 401)
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
                                dbContext.WeatherEntries.Add(weatherEntry);
                            }
                        }

                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"Hava durumu verisi başarıyla veritabanına kaydedildi.");
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Hava durumu verisi çekilirken bir hata oluştu.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("Hava durumu otomatik çekme servisi durduruldu.");
        }
    }
}