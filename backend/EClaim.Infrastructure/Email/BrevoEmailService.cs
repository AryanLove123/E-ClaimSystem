using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EClaim.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EClaim.Infrastructure;

public class BrevoEmailService : IEmailService
{
    private HttpClient _httpClient;
    private IConfiguration _configuration;
    private ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(HttpClient httpClient, IConfiguration configuration, ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        try
        {
            var emailSection = _configuration.GetSection("Email");
            var apiKey = emailSection["BrevoApiKey"];
            var fromEmail = emailSection["FromAddress"] ?? "no-reply@eclaim.local";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Brevo API key not configured; email to {To} was not sent.", toEmail);
                return false;
            }

            var payload = new
            {
                sender = new { email = fromEmail, name = "E-Claim System" },
                to = new[] { new { email = toEmail } },
                subject,
                textContent = body
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Brevo to {To}", toEmail);
            return false;
        }
    }
}
