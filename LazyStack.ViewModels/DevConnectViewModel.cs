using System.Reactive;
using ReactiveUI;
using LazyStackBase;


namespace LazyStack.ViewModels;
public class DevConnectViewModel : ReactiveObject
{
    //public DevConnectViewModel(IStacksConfig stacksConfig)
    //{
    //    this.stacksConfig = stacksConfig;
    //    // Commands
    //    SelectAPI = ReactiveCommand.CreateFromTask<string>(SelectAPIAsync);
    //    SelectAssets = ReactiveCommand.CreateFromTask<string>(SelectAssetsAsync);
    //}

    //private IStacksConfig stacksConfig;
    //public RunConfig RunConfig { get { return stacksConfig.CurrentStack.RunConfig; } }
    //public ReactiveCommand<string, Unit> SelectAPI { get; init; }
    //public ReactiveCommand<string, Unit> SelectAssets { get; init; }
    //public Dictionary<string,string> Tenants => stacksConfig.CurrentStack.TenantConfig.Tenants;

    //private Task SelectAPIAsync(string api)
    //{
    //    RunConfig.Apis = api;
    //    return Task.CompletedTask;
    //}

    //private Task SelectAssetsAsync(string assets)
    //{
    //    RunConfig.Assets = assets;
    //    return Task.CompletedTask;
    //}
}
