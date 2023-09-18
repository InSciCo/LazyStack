namespace LazyStackBase;

public interface IInternetConnectivitySvc : IDisposable
{
    event Action<bool> NetworkStatusChanged;
    bool IsOnline { get; }
    Task<bool> CheckInternetConnectivityAsync();
}
