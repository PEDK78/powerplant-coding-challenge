using Engie.PCC.Api.Models;
using Engie.PCC.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Middlewares
{
    public class ApiExceptionHandlerMiddleware
    {
        private readonly ILogger _logger;

        private readonly RequestDelegate _next;
        private readonly Func<object, Task> _clearCacheHeadersDelegate;

        public ApiExceptionHandlerMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ApiExceptionHandlerMiddleware>();
            _clearCacheHeadersDelegate = ClearCacheHeaders;
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
            if (exception == null) return;

            _logger.LogError(0, exception, "An unhandled exception has occurred: " + exception.Message);
            // We can't do anything if the response has already started, just abort.
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the error handler will not be executed.");
                throw exception;
            }

            try
            {
                context.Response.Clear();
                context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);

                await WriteExceptionAsync(context, exception).ConfigureAwait(false);

                return;
            }
            catch (Exception ex2)
            {
                // Suppress secondary exceptions, re-throw the original.
                _logger.LogError(0, ex2, "An exception was thrown attempting to execute the error handler.");
            }

            throw exception; // Re-throw the original if we couldn't handle it
        }

        private static async Task WriteExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string errmsg;
            IEnumerable<string> errDetails = null;

            var exceptionType = exception.GetType();
            if (exceptionType == typeof(ProductionPlanServiceException))
            {
                errmsg = exception.Message;
                status = HttpStatusCode.BadRequest;
            }
            else
            {
                errmsg = exception.Message;
                status = HttpStatusCode.InternalServerError;
            }

            HttpResponse response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)status;

            await response.WriteAsync(JsonConvert.SerializeObject(new ApiError
            {
                Message = errmsg,
                Errors = errDetails,
                FullException = exception.StackTrace
            })).ConfigureAwait(false);
        }

        private Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.CompletedTask;
        }
    }
}
