using Newtonsoft.Json;
using StajApi.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StajApi.Core.Infrastructure.Utilities
{
    public class RoarResponse<T> : IRoarResponse
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("responseType")]
        public RoarResponseCodeType ResponseType { get; set; }

        [JsonProperty("httpStatusCode")]
        public int HttpStatusCode { get; set; }

        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }

        [JsonProperty("exception")]
        public Exception Exception { get; set; }

        public RoarResponse() { }

        public RoarResponse(bool isSuccess, string message, T data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            ResponseType = SetResponseType(isSuccess);
        }

        public RoarResponse(bool isSuccess, T data)
        {
            IsSuccess = isSuccess;
            Data = data;
            ResponseType = SetResponseType(isSuccess);
        }

        public RoarResponse(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
            ResponseType = SetResponseType(isSuccess);
        }

        public RoarResponse(bool isSuccess, string message, RoarResponseCodeType responseType)
        {
            IsSuccess = isSuccess;
            Message = message;
            ResponseType = responseType;
        }

        public RoarResponse(Exception ex, string message)
        {
            IsSuccess = false;
            Message = message + " " + ex.Message;
            ResponseType = SetResponseType(false);
            Exception = ex;
        }

        private RoarResponseCodeType SetResponseType(bool isSuccess, RoarResponseCodeType? roarResponseCodeType = null)
        {
            RoarResponseCodeType responseCode;

            if (isSuccess)
                responseCode = !roarResponseCodeType.HasValue ? RoarResponseCodeType.Success : roarResponseCodeType.Value;
            else
                responseCode = !roarResponseCodeType.HasValue ? RoarResponseCodeType.Error : roarResponseCodeType.Value;

            return responseCode;
        }

        public RoarResponse(Exception exception)
        {
            IsSuccess = false;
            Exception = exception;
            Message = exception.Message;
            ResponseType = SetResponseType(false);
        }
    }

    public static class RoarResponseHelpers // RoarResponse yerine RoarResponseHelpers kullanıyoruz
    {
        public static RoarResponse<T> Success<T>(T data, string message = "İşlem başarıyla tamamlandı.")
        {
            return new RoarResponse<T> { IsSuccess = true, Message = message, Data = data, ResponseType = RoarResponseCodeType.Success, HttpStatusCode = StatusCodes.Status200OK };
        }

        public static RoarResponse<T> Success<T>(string message = "İşlem başarıyla tamamlandı.")
        {
            return new RoarResponse<T> { IsSuccess = true, Message = message, Data = default(T), ResponseType = RoarResponseCodeType.Success, HttpStatusCode = StatusCodes.Status200OK };
        }

        public static RoarResponse<T> Error<T>(string message = "Bir hata oluştu.", Exception ex = null, RoarResponseCodeType responseType = RoarResponseCodeType.Error, int httpStatusCode = StatusCodes.Status400BadRequest)
        {
            return new RoarResponse<T> { IsSuccess = false, Message = message, ResponseType = responseType, Exception = ex, HttpStatusCode = httpStatusCode };
        }

        public static RoarResponse<T> Error<T>(Exception ex, string message = "Bir hata oluştu.", int httpStatusCode = StatusCodes.Status500InternalServerError)
        {
            return new RoarResponse<T> { IsSuccess = false, Message = message, Exception = ex, ResponseType = RoarResponseCodeType.Error, HttpStatusCode = httpStatusCode };
        }

        public static RoarResponse<List<T>> SuccessList<T>(List<T> data, string message = "Liste başarıyla getirildi.", int httpStatusCode = StatusCodes.Status200OK)
        {
            var response = new RoarResponse<List<T>> { IsSuccess = true, Message = message, Data = data, ResponseType = RoarResponseCodeType.Success, HttpStatusCode = httpStatusCode };
            response.TotalCount = data?.Count() ?? 0;
            return response;
        }
    }
}
