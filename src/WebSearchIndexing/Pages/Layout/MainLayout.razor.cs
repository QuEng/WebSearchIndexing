using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Security.Cryptography;
using System.Text;
using WebSearchIndexing.Extensions;
using WebSearchIndexing.Theming;

namespace WebSearchIndexing.Pages.Layout;

public partial class MainLayout : LayoutComponentBase
{
    private bool _isInitialized = false;
    private bool _drawerOpen = false;
    private bool _isDarkMode;
    private bool _isCanShowContent = false;
    private bool _hasAccess = false;
    private string _accessKey = string.Empty;

    private CustomThemeProvider _mudThemeProvider;
    private MudTheme _customTheme = new GlobalTheme();

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private string AccessKey => Configuration["ApplicationAccessKey"]!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var encryptedKey = await JsRuntime.InvokeAsync<string>("localStorage.getItem", "accessKey");
            if (!string.IsNullOrWhiteSpace(encryptedKey) && IsValidKey(Decrypt(encryptedKey)))
            {
                _hasAccess = true;
            }
            _isInitialized = Configuration.GetInitializedState();
            _isDarkMode = await _mudThemeProvider.GetSystemPreference();
            _isCanShowContent = true;
            StateHasChanged();
            await _mudThemeProvider.WatchSystemPreference(OnSystemPreferenceChanged);
        }
    }

    private async Task OnSystemPreferenceChanged(bool newValue)
    {
        _isDarkMode = newValue;
        StateHasChanged();
    }

    private void ChangeInitializedState()
    {
        _isInitialized = true;
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void SubmitAccessKey()
    {
        if (string.IsNullOrWhiteSpace(_accessKey))
        {
            Snackbar.Add("Access key is required", Severity.Error);
            return;
        }
        if (IsValidKey(_accessKey))
        {
            _hasAccess = true;
            JsRuntime.InvokeVoidAsync("localStorage.setItem", "accessKey", Encrypt(_accessKey));
            _accessKey = string.Empty;
            StateHasChanged();
        }
        else
        {
            _accessKey = string.Empty;
            Snackbar.Add("Access key is invalid", Severity.Error);
        }
    }

    private bool IsValidKey(string key) => AccessKey == key;

    private string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;
        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(AccessKey);
            aesAlg.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using MemoryStream msEncrypt = new MemoryStream();
            msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
            msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                csEncrypt.FlushFinalBlock();
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch
        {
            return string.Empty;
        }
    }

    private string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText)) return string.Empty;
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(AccessKey);
                aesAlg.Mode = CipherMode.CBC;

                int ivLength = BitConverter.ToInt32(cipherBytes, 0);
                byte[] iv = new byte[ivLength];
                Array.Copy(cipherBytes, sizeof(int), iv, 0, ivLength);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv);
                using MemoryStream msDecrypt = new MemoryStream(cipherBytes, ivLength + sizeof(int), cipherBytes.Length - ivLength - sizeof(int));
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                byte[] decryptedBytes = new byte[cipherBytes.Length - ivLength - sizeof(int)];
                int bytesRead = csDecrypt.Read(decryptedBytes, 0, decryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes, 0, bytesRead);
            }
        }
        catch
        {
            return string.Empty;
        }
    }
}