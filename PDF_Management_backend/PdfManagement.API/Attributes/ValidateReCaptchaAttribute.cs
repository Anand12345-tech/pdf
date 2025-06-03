using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PdfManagement.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateReCaptchaAttribute : ActionFilterAttribute
    {
        private const string RECAPTCHA_RESPONSE_KEY = "g-recaptcha-response";
        
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Skip validation in development environment
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                await next();
                return;
            }
            
            var configuration = (IConfiguration)context.HttpContext.RequestServices.GetService(typeof(IConfiguration));
            var reCaptchaSecret = configuration["Security:ReCaptchaSecretKey"];
            
            // If reCaptcha is not configured, skip validation
            if (string.IsNullOrEmpty(reCaptchaSecret))
            {
                await next();
                return;
            }
            
            // Get reCaptcha response from request
            var request = context.HttpContext.Request;
            string reCaptchaResponse = null;
            
            if (request.Headers.ContainsKey(RECAPTCHA_RESPONSE_KEY))
            {
                reCaptchaResponse = request.Headers[RECAPTCHA_RESPONSE_KEY];
            }
            else if (request.Form.ContainsKey(RECAPTCHA_RESPONSE_KEY))
            {
                reCaptchaResponse = request.Form[RECAPTCHA_RESPONSE_KEY];
            }
            
            // If no reCaptcha response, return bad request
            if (string.IsNullOrEmpty(reCaptchaResponse))
            {
                context.Result = new BadRequestObjectResult(new { success = false, message = "reCAPTCHA validation failed" });
                return;
            }
            
            // Validate reCaptcha
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={reCaptchaSecret}&response={reCaptchaResponse}");
                
                var reCaptchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(response);
                
                if (!reCaptchaResult.Success)
                {
                    context.Result = new BadRequestObjectResult(new { success = false, message = "reCAPTCHA validation failed" });
                    return;
                }
            }
            
            await next();
        }
        
        private class ReCaptchaResponse
        {
            public bool Success { get; set; }
            public string[] ErrorCodes { get; set; }
        }
    }
}
