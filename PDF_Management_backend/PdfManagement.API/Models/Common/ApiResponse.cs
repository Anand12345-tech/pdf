namespace PdfManagement.API.Models.Common
{
    /// <summary>
    /// Generic API response
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic API response with data
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }
}
