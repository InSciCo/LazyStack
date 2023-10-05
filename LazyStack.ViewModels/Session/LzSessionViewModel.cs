namespace LazyStack.ViewModels;

/// <summary>
/// Orchastrates the connection to services.
/// Decouples IAuthProcess from ILzClientConfig.
/// </summary>
public class LzSessionViewModel : LzViewModelBase, ILzSessionViewModel
{
    public LzSessionViewModel(
        IAuthProcess authProcess,
        ILzClientConfig clientConfig,
        IInternetConnectivitySvc internetConnectivity
        )
    {
        AuthProcess = authProcess ?? throw new ArgumentNullException(nameof(authProcess));
        ClientConfig = clientConfig ?? throw new ArgumentNullException(nameof(clientConfig));
        this.internetConnectivity = internetConnectivity ?? throw new ArgumentNullException(nameof(internetConnectivity));

        // ReactiveUI statements. I've added some comments for folks who have 
        // never used ReactiveUI
        // When Authprocess.IsSignedIn changes, update this classes observable IsSignedIn property
        this.WhenAnyValue(x => x.AuthProcess.IsSignedIn)
            .ToPropertyEx(this, x => x.IsSignedIn);

        // When Authprocess.IsBusy changes, update this classes observable IsBusy property
        this.WhenAnyValue(x => x.AuthProcess.IsBusy)
            .ToProperty(this, x => x.IsBusy);

        // When Authprocess.HasChallenge changes, update this classes observable HasChallenge property
        this.WhenAnyValue(x => x.AuthProcess.HasChallenge)
            .ToPropertyEx(this, x => x.HasChallenge);

        // When the login changes, update the IsAdmin property. Note the virtual method IsAdminCheck().
        // Implement your application specific logic in the IsAdminCheck() method.  
        this.WhenAnyValue(x => x.AuthProcess.Login)
            .SelectMany(async x => await IsAdminCheck()) // SelectMany allows the use of async in the reactive chain
            .ToPropertyEx(this, x => x.IsAdmin);

        this.WhenAnyValue(x => x.internetConnectivity.IsOnline)
            .ToPropertyEx(this, x => x.IsOnline);

        this.WhenAnyValue(x => x.IsSignedIn)
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Subscribe(async (isSignedIn) =>
            {
                if (isSignedIn)
                    await OnSignedInAsync();
                else
                    await OnSignedOutAsync();
            });

    }
    protected readonly IInternetConnectivitySvc internetConnectivity;

    public IAuthProcess AuthProcess { get; set; }
    public ILzClientConfig ClientConfig { get; set; }

    // The ObservableAsProperty annotation is defined in ReactiveUI.Fody
    [ObservableAsProperty] public bool IsSignedIn { get; }
    [Reactive] public bool IsBusy { get; set; }
    [ObservableAsProperty] public bool HasChallenge { get; }
    [ObservableAsProperty] public bool IsAdmin { get; }
    [ObservableAsProperty] public bool IsOnline { get; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsLoaded { get; set; }
    public async virtual Task<bool> IsAdminCheck()
    {
        await Task.Delay(0);
        // This is not recommended. Override this method in your app 
        // to do an appropriate check.
        return AuthProcess.Login == "Administrator";
    }
    public Task<bool> CheckInternetConnectivityAsync()
    {
        return internetConnectivity.CheckInternetConnectivityAsync();
    }
    public virtual async Task OnSignedInAsync()
    {
        try
        {
            IsBusy = true;
            IsLoading = true;
            await LoadAsync();
            IsLoaded = true;

        } catch
        {
            return;
        } finally
        {
            IsBusy = false;
            IsLoading = false;
        }
    }
    public virtual async Task LoadAsync()
    {
        await Task.Delay(0);
    }
    public virtual async Task OnSignedOutAsync()
    {
        await UnloadAsync();    
    }
    public virtual async Task UnloadAsync()
    { 
        await Task.Delay(0);    
    }
}
