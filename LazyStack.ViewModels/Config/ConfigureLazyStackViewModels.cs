using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LazyStackAuthV2;
using LazyStack.Utils;
using System.Reflection;

namespace LazyStack.ViewModels;

public static class ConfigureLazyStackViewModels
{
    public static IServiceCollection AddLazyStackViewModels(this IServiceCollection services)
    {
        return services
            //.AddSingleton<DevConnectViewModel>()
            ;
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
