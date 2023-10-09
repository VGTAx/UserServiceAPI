using System.Text;

namespace UserServiceAPI.Middlewares
{
  /// <summary>
  /// Middleware for logging HTTP requests and responses.
  /// </summary>
  public class HttpLoggingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpLoggingMiddleware> _logger;
    /// <summary>
    /// RequestResponseLoggingMiddleware constructor
    /// </summary>
    /// <param name="next">The following middleware in the request pipeline.</param>
    /// <param name="logger">Logger for recording logs.</param>    
    public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {      
      LogRequest(context);
      
      await _next(context);
      
      LogResponse(context);
    }

    private void LogRequest(HttpContext context)
    {      
      _logger.LogInformation("Request: HTTP {method} {path}", context.Request.Method, context.Request.Path);
    }

    private void LogResponse(HttpContext context)
    {
      switch (context.Response.StatusCode / 100)
      {
        case 2:
          _logger.LogInformation("Request: HTTP {method} {path}", context.Request.Method, context.Request.Path);
          break;
        case 4:
          _logger.LogWarning("Request: HTTP {method} {path}", context.Request.Method, context.Request.Path);
          break;
        case 5:
          _logger.LogError("Request: HTTP {method} {path}", context.Request.Method, context.Request.Path);
          break;
      }
    }
  }
}
