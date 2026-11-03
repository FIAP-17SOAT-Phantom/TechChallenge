using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Options;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Infrastructure.Notifications;

public sealed class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailOptions _options;

    public ResendEmailSender(HttpClient httpClient, IOptions<EmailOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ResendApiKey))
            throw new InvalidOperationException("Email:ResendApiKey nao configurado");

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var request = CreateRequest(message);
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (attempt == 3 || !IsTransient(response.StatusCode))
                {
                    throw new InvalidOperationException($"Falha ao enviar e-mail pelo Resend ({(int)response.StatusCode}): {Limitar(responseBody, 500)}");
                }
            }
            catch (HttpRequestException) when (attempt < 3)
            {
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < 3)
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
        }
    }

    private HttpRequestMessage CreateRequest(EmailMessage message)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ResendApiKey);

        if (!string.IsNullOrWhiteSpace(message.IdempotencyKey))
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", message.IdempotencyKey);
        }

        request.Content = JsonContent.Create(new { from = _options.From, to = new[] { message.Recipient }, subject = message.Subject, text = message.TextBody });
        return request;
    }

    private static bool IsTransient(HttpStatusCode statusCode) => statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
    private static string Limitar(string value, int length) => value.Length <= length ? value : value[..length];
}
