using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shared.SeedWork
{
    public static class JsonUtil
    {
        private enum StatusResponse
        {
            Success = 1,
            Error = 0
        }

        public static ObjectResult Success(object data = null, string? message = null,
            int? dataCount = null)
        {
            object obj;

            if (dataCount.HasValue)
                obj = new { statusCode = StatusCodes.Status200OK, isSuccess = StatusResponse.Success, message, dataCount, data };
            else
                obj = new { statusCode = StatusCodes.Status200OK, isSuccess = StatusResponse.Success, message, data };

            return new ObjectResult(obj)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        private static ObjectResult Error(int? status, string? code, string? message, object data)
        {
            return new ObjectResult(new { status, isSuccess = StatusResponse.Error, code, message, data })
            {
                StatusCode = status
            };
        }

        public static ObjectResult Errors(int status, string code, List<ValidationFailure> errors)
        {
            var validation = errors.Select(x =>
                new
                {
                    x.ErrorCode,
                    target = x.PropertyName,
                    message = x.ErrorMessage
                });
            return new ObjectResult(new { status, isSuccess = StatusResponse.Error, code, validation })
            {
                StatusCode = status
            };
        }

        public static ObjectResult Error(int? status = null, string? code = null, string? message = null)
        {
            return Error(status, code, message, null);
        }
    }
}
