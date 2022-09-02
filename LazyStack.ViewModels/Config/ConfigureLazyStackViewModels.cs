using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LazyStackAuthV2;
using LazyStack.Utils;

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
        messages.AddlazyStackAuth();
        // Add/Overwrite messages with messages in this library's Messages.json
        using var messagesStream = typeof(ConfigureLazyStackViewModels).Assembly.GetManifestResourceStream("LazyStack.ViewModels.Config.Messages.json")!;
        if (messagesStream != null)
        {
            using var messagesReader = new StreamReader(messagesStream);
            var messagesText = messagesReader.ReadToEnd();
            messages.MergeJson(messagesText);
        }
        return messages;
    }
}
