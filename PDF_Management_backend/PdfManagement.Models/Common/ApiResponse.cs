using Swashbuckle.AspNetCore.Annotations;

namespace PdfManagement.Models.Common
{
    public class ApiResponse
    {
        [SwaggerSchema(Description = "Indicates if the operation was successful")]
        public bool Success { get; set; }

        [SwaggerSchema(Description = "Message describing the result of the operation")]
        public string Message { get; set; } = string.Empty;
    }

    public class ApiResponse<T> : ApiResponse
    {
        [SwaggerSchema(Description = "Data returned from the operation")]
        public T? Data { get; set; }
    }
}
