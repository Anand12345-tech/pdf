using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PdfManagement.API
{
    [Route("api")]
    [ApiController]
    public class VercelEntryPoint : ControllerBase
    {
        private readonly ILogger<VercelEntryPoint> _logger;

        public VercelEntryPoint(ILogger<VercelEntryPoint> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Vercel serverless function entry point hit");
            return Ok(new { message = "PDF Management API is running", timestamp = DateTime.UtcNow });
        }
    }
}
