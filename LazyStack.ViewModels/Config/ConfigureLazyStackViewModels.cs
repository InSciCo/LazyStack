using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using LazyStackAuthV2;
using LazyStack.Utils;

namespace LazyStack.ViewModels;

public static class ConfigureLazyStackViewModels
{
    public static IServiceCollection AddLazyStackViewModels(this IServiceCollection services)
    {
        services.TryAddSingleton<DevConnectViewModel>();
        return services;
    }
    public static IMessages AddLazyStackViewModels(this IMessages messages)
    {
        var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
        var assemblyName = assembly!.GetName().Name;

        messages.AddlazyStackAuth();
        // Add/Overwrite messages with messages in this library's Messages.json
        using var messagesStream = assembly?.GetManifestResourceStream($"{assemblyName}.Config.Messages.json")!;
        if (messagesStream != null)
        {
            using var messagesReader = new StreamReader(messagesStream);
            var messagesText = messagesReader.ReadToEnd();
            messages.MergeJson(messagesText);
        }
        return messages;
    }
}
