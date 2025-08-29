using StajApi.Services;
using StajApi.Core.Infrastructure.Utilities;
using StajApi.Core.ExceptionHandler;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc; // ApiBehaviorOptions için gerekli
using System.Linq; // SelectMany için
using StajApi.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// InMemory veritabanını ekleme
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("WeatherDb"));

// API davranışını yapılandırıyoruz
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Model doğrulama başarısız olduğunda varsayılan 400 Bad Request yanıtını engelle
    options.InvalidModelStateResponseFactory = context =>
    {
        // ModelState'deki tüm hata mesajlarını topla
        var errors = context.ModelState.Where(x => x.Value.Errors.Any())
                                       .SelectMany(x => x.Value.Errors)
                                       .Select(x => x.ErrorMessage)
                                       .ToList();

        // RoarResponse formatında bir hata yanıtı oluştur
        var roarResponse = new RoarResponse<object> // T burada object olabilir çünkü veri dönmüyoruz
        {
            IsSuccess = false,
            Message = "Gönderilen istekte doğrulama hataları var.",
            Content = string.Join(" | ", errors), // Hataları Content olarak ekliyoruz
            ResponseType = StajApi.Entities.Enums.RoarResponseCodeType.ValidationError,
            HttpStatusCode = StatusCodes.Status400BadRequest // Yine de 400 Bad Request
        };

        // Custom RoarResponse'unuzu HTTP 200 OK ile döndür
        return new OkObjectResult(roarResponse);
    };
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddLogging();

builder.Services.AddSingleton<IHostedService, WeatherSchedulerService>();


var app = builder.Build();





if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();