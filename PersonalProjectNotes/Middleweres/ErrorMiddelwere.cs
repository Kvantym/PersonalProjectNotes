using Newtonsoft.Json;
using PersonalProjectNotes.Services.Exceptions;
using PersonalProjectNotes.Services.Exceptions.Response;
using System.Net;

namespace PersonalProjectNotes.Middleweres
{
    public class ErrorMiddelwere
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorMiddelwere> _logger;

        public ErrorMiddelwere(RequestDelegate next, ILogger<ErrorMiddelwere> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

   private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError($"Error occurred: {exception}");
            switch (exception)
            {
                case BadRequestException badRequestException:
                    await WriteErrorAsync(context, HttpStatusCode.BadRequest, badRequestException.Message);
                    break;
                case NotFoundException notFoundException:
                    await WriteErrorAsync(context, HttpStatusCode.NotFound, notFoundException.Message);
                    break;
                case AuthorizationException authorizationException:
                    await WriteErrorAsync(context, HttpStatusCode.Unauthorized, authorizationException.Message);
                    break;
                case RegistrationException registrationException:
                    await WriteErrorAsync(context, HttpStatusCode.Conflict, registrationException.Message);
                    break;
                case ConflictException conflictException:
                    await WriteErrorAsync(context, HttpStatusCode.Conflict, conflictException.Message);
                    break;
                default:
                    await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
                    break;
            }
        }

        private async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            var result = JsonConvert.SerializeObject(new ExceptionRespons { Error = message });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(result);
        }
    }
}
