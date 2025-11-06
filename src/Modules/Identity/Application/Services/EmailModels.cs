using Newtonsoft.Json;

namespace WebSearchIndexing.Modules.Identity.Application.Services;

public class EmailRequestAddress
{
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}

public record EmailRequest
{
    [JsonProperty("from")]
    public EmailRequestAddress From { get; set; } = new();

    [JsonProperty("to")]
    public EmailRequestAddress[] To { get; set; } = [];

    [JsonProperty("subject")]
    public string Subject { get; set; } = "";
}

public record EmailRequestWithBody : EmailRequest
{
    [JsonProperty("text")]
    public string TextBody { get; set; } = "";

    [JsonProperty("html")]
    public string? HtmlBody { get; set; } = "";
}
