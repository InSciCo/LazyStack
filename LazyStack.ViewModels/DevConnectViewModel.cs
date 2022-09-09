using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using LazyStack.Utils;
using LazyStackAuthV2;


namespace LazyStack.ViewModels;
public class DevConnectViewModel : ReactiveObject
{
    public DevConnectViewModel(IStacksConfig stacksConfig)
    {
        this.stacksConfig = stacksConfig;
        // Commands
        SelectAPI = ReactiveCommand.CreateFromTask<string>(SelectAPIAsync);
        SelectAssets = ReactiveCommand.CreateFromTask<string>(SelectAssetsAsync);
    }

    private IStacksConfig stacksConfig;
    public RunConfig RunConfig { get { return stacksConfig.CurrentStack.RunConfig; } }
    public ReactiveCommand<string, Unit> SelectAPI { get; init; }
    public ReactiveCommand<string, Unit> SelectAssets { get; init; }


    private Task SelectAPIAsync(string api)
    {
        RunConfig.Apis = api;
        return Task.CompletedTask;
    }

    private Task SelectAssetsAsync(string assets)
    {
        RunConfig.Assets = assets;
        return Task.CompletedTask;
    }
}
