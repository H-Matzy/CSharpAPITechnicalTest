using System.Text;
using System.Text.Json;
using ControlExpertSimpleAPI.Models;
using Newtonsoft.Json;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = await FormatRequest(context.Request);
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var startTime = DateTime.UtcNow;
        await _next(context);
        var endTime = DateTime.UtcNow;

        var response = await FormatResponse(context.Response);

        var requestHeaders = context.Request.Headers;
        var headers = System.Text.Json.JsonSerializer.Serialize(requestHeaders.ToDictionary(h => h.Key, h => h.Value.ToString()),
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        _logger.LogInformation($@"API Call Log:
        Request Headers: {headers}
        Request Body: {request.Body}
        Response Content: {response}
        API Start Time: {startTime:o}
        API End Time: {endTime:o}
        Client IP Address: {context.Connection.RemoteIpAddress}
        Host IP Address: {context.Connection.LocalIpAddress}");

        await responseBody.CopyToAsync(originalBodyStream);
    }


    private async Task<RequestData> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();

        var body = string.Empty;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        return new RequestData
        {
            Body = body,
            Headers = request.Headers
        };
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        string text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return $"{response.StatusCode}: {text}";
    }
}
