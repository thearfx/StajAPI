using System;

namespace StajApi.Core.Infrastructure.Utilities
{
    public interface IRoarResponse
    {
        bool IsSuccess { get; }
        string Message { get; }
    }
}