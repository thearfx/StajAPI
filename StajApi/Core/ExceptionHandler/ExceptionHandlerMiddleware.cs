using Microsoft.AspNetCore.Http;
using StajApi.Core.Infrastructure.Utilities;
using StajApi.Entities.Enums;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace StajApi.Core.ExceptionHandler
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Request Başlangıç Loglaması
                _logger.LogInformation("Middleware: Request başladı. Path: {Path}", httpContext.Request.Path);

                await _next(httpContext);

                // Request Bitiş Loglaması
                _logger.LogInformation("Middleware: Request tamamlandı. Status Code: {StatusCode}", httpContext.Response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen bir hata oluştu: {ErrorMessage}", ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new RoarResponse<string>
            {
                IsSuccess = false,
                Message = "İşlem sırasında beklenmeyen bir hata oluştu.",
                Data = null,
                ResponseType = RoarResponseCodeType.Error,
                HttpStatusCode = (int)HttpStatusCode.InternalServerError,
                Exception = ex
            };

            var jsonResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });

            await httpContext.Response.WriteAsync(jsonResponse);
        }
    }
}