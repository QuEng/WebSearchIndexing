using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using MudBlazor;

namespace WebSearchIndexing.Modules.Core.Ui.Components;

public partial class AccessComponent : ComponentBase
{
    private const string StorageAccessKey = "accessKey";
    private string _accessKey = string.Empty;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback AccessAllowed { get; set; }

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private string ApplicationAccessKey => Configuration["ApplicationAccessKey"]!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (await IsValidKeyInStorageAsync())
        {
            await AccessAllowed.InvokeAsync();
        }
    }

    private async Task SubmitAccessKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(_accessKey))
        {
            Snackbar.Add("Access key is required", Severity.Error);
            return;
        }

        var hashedInputKey = HashAccessKey(_accessKey);
        if (IsValidKey(hashedInputKey))
        {
            await JsRuntime.InvokeVoidAsync("localStorage.setItem", StorageAccessKey, hashedInputKey);
            _accessKey = string.Empty;
            await AccessAllowed.InvokeAsync();
        }
        else
        {
            _accessKey = string.Empty;
            Snackbar.Add("Access key is invalid", Severity.Error);
        }
    }

    private async Task<bool> IsValidKeyInStorageAsync()
    {
        var hashedStorageKey = await JsRuntime.InvokeAsync<string>("localStorage.getItem", StorageAccessKey);
        if (string.IsNullOrWhiteSpace(hashedStorageKey))
        {
            return false;
        }

        return IsValidKey(hashedStorageKey);
    }

    private bool IsValidKey(string hashedKey)
        => hashedKey == HashAccessKey(ApplicationAccessKey);

    private static string HashAccessKey(string plainText)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        byte[] hashBytes = sha256.ComputeHash(bytes);

        StringBuilder builder = new();
        foreach (byte b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
