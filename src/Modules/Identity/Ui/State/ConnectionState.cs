namespace WebSearchIndexing.Modules.Identity.Ui.State;

public class ConnectionState
{
    public bool IsConnected { get; private set; } = true;
    public DateTime? LastConnectedAt { get; private set; }
    public DateTime? LastFailedAt { get; private set; }
    public string? LastError { get; private set; }
    public int FailedAttempts { get; private set; }
    
    public bool IsReconnecting => !IsConnected && FailedAttempts > 0;
    public TimeSpan? TimeSinceLastConnection => LastConnectedAt?.Subtract(DateTime.UtcNow);
    public TimeSpan? TimeSinceLastFailure => LastFailedAt?.Subtract(DateTime.UtcNow);
    
    public event Action<bool>? ConnectionStateChanged;
    
    public void SetConnected()
    {
        var wasConnected = IsConnected;
        IsConnected = true;
        LastConnectedAt = DateTime.UtcNow;
        LastError = null;
        FailedAttempts = 0;
        
        if (!wasConnected)
        {
            ConnectionStateChanged?.Invoke(true);
        }
    }
    
    public void SetDisconnected(string? error = null)
    {
        var wasConnected = IsConnected;
        IsConnected = false;
        LastFailedAt = DateTime.UtcNow;
        LastError = error;
        FailedAttempts++;
        
        if (wasConnected)
        {
            ConnectionStateChanged?.Invoke(false);
        }
    }
    
    public void Reset()
    {
        IsConnected = true;
        LastConnectedAt = null;
        LastFailedAt = null;
        LastError = null;
        FailedAttempts = 0;
    }
}
