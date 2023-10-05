namespace LazyStack.ViewModels
{
    public interface ILzSessionViewModel
    {
        IAuthProcess AuthProcess { get; set; }
        ILzClientConfig ClientConfig { get; set; }
        bool HasChallenge { get; }
        bool IsAdmin { get; }
        bool IsBusy { get; }
        bool IsOnline { get; }
        bool IsSignedIn { get; }
        bool IsLoaded { get; set; }
        bool IsLoading { get; set; }
        Task<bool> IsAdminCheck();
        Task<bool> CheckInternetConnectivityAsync();
        Task LoadAsync();
        Task OnSignedInAsync();
        Task OnSignedOutAsync();
        Task UnloadAsync();
    }
}