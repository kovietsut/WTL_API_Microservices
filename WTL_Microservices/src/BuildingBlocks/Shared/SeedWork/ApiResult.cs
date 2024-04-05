using System.Text.Json.Serialization;

namespace Shared.SeedWork
{
    public class ApiResult<T>
    {
        public bool IsSucceeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
