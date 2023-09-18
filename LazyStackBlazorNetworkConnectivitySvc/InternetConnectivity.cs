using Microsoft.JSInterop;
using LazyStackBase;
using Microsoft.JSInterop.Infrastructure;

namespace LazyStackBlazorNetworkConnectivitySvc;

/// <summary>
/// Monitor Internet connectivity using browser's navigator.offline and navigator.online events.
/// Inject this class early on in the app setup.
/// Make a single call to CheckInternetConnectivityAsync() to start listening for events.
/// In general, use ReactiveUI subscriptions to respond to changes in the IsOnline value.
/// Even through this is a Razor library, there is no component. This is a service that 
/// makes use of JSInterop. However, since it use JSInterop, you need to be using 
/// Blazor! 
/// </summary>
public class InternetConnectivity : NotifyBase, IInternetConnectivitySvc, IDisposable
{
    public InternetConnectivity(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/LazyStackBlazorNetworkConnectivitySvc/internetConnectivity.js").AsTask());
    }
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    private bool isOnline; 
    public bool IsOnline { 
        get => isOnline;  
        private set => SetProperty(ref isOnline, value);
    }
    public event Action<bool>? NetworkStatusChanged;
    private DotNetObjectReference<InternetConnectivity>? dotNetReference;
    
    private async Task Initialize()
    {
        var jsRuntime = await moduleTask.Value;
        if(dotNetReference == null) {
            dotNetReference = DotNetObjectReference.Create(this);
            await jsRuntime.InvokeVoidAsync("initializeInternetStatusInterop", dotNetReference);
        }
    }
    public async Task<bool> CheckInternetConnectivityAsync()
    {
        var jsRuntime = await moduleTask.Value;   
        await Initialize();
        IsOnline = await jsRuntime.InvokeAsync<bool>("checkInternetConnectivity");
        return IsOnline;
    }
    [JSInvokable]
    public void HandleNetworkStatusChange(bool online)
    {
        IsOnline = online;
    }
    public async void Dispose()
    {
        if(moduleTask.IsValueCreated)
        {
            var jsRuntime = await moduleTask.Value;
            // Dispose of the DotNetObjectReference and remove event listeners
            await jsRuntime.InvokeVoidAsync("removeInternetStatusInterop");
            dotNetReference?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}