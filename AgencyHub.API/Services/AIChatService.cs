using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AgencyHub.API.Services;

public sealed class AIChatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AIChatService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> AskAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Message cannot be empty.",
                nameof(message));
        }

        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured.");
        }

        var model =
            _configuration["OpenAI:Model"]
            ?? "gpt-6-astra";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/responses");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        var requestBody = new
        {
            model = model,

            input = new object[]
            {
                new
                {
                    role = "system",

                    content =
                        "You are the professional AI assistant for AgencyHub, " +
                        "a multi-tenant agency management platform. " +
                        "Be concise, professional and helpful. " +
                        "Do not invent database information. " +
                        "If database information is unavailable, clearly say so."
                },

                new
                {
                    role = "user",

                    content = message
                }
            },

            max_output_tokens = 500
        };

        var json = JsonSerializer.Serialize(requestBody);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenAI API error. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }

        using var document =
            JsonDocument.Parse(responseBody);

        if (document.RootElement.TryGetProperty(
                "output_text",
                out var outputText))
        {
            var result = outputText.GetString();

            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
        }

        if (document.RootElement.TryGetProperty(
                "output",
                out var output))
        {
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty(
                        "content",
                        out var content))
                {
                    continue;
                }

                foreach (var contentItem in
                         content.EnumerateArray())
                {
                    if (!contentItem.TryGetProperty(
                            "text",
                            out var text))
                    {
                        continue;
                    }

                    var result = text.GetString();

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
        }

        return "The AI returned an empty response.";
    }
}