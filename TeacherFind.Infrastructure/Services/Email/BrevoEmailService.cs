using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeacherFind.Application.Abstractions.Services;

namespace TeacherFind.Infrastructure.Services.Email;

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly EmailOptions _options;
    private readonly ILogger<BrevoEmailService> _logger;

    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    public BrevoEmailService(
        HttpClient httpClient,
        IOptions<EmailOptions> options,
        ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Email:ApiKey yapılandırılmamış. Mail gönderilmedi: {ToEmail}", toEmail);
            return;
        }

        var payload = new
        {
            sender = new
            {
                name = _options.FromName,
                email = _options.FromEmail
            },
            to = new[]
            {
                new { email = toEmail }
            },
            subject = subject,
            htmlContent = htmlBody,
            textContent = textBody
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(BrevoApiUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Brevo mail gönderilemedi. Status: {Status}, Body: {Body}",
                response.StatusCode, body);

            throw new InvalidOperationException(
                $"Mail gönderilemedi. HTTP {(int)response.StatusCode}");
        }

        _logger.LogInformation("Mail gönderildi: {ToEmail}, Konu: {Subject}", toEmail, subject);
    }
}