using LazyStack.Utils;
using LazyStackAuthV2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LazyStack.Components;

public static class ConfigureLazyStackComponents
{
    public static IServiceCollection AddLazyStackComponents(this IServiceCollection services)
    {
        return services
            //.AddTransient(typeof(BecknVersion))
            ;
    }

    public static IMessages AddLazyStackComponents(this IMessages messages)
    {
        using var messagesStream = typeof(ConfigureLazyStackComponents).Assembly.GetManifestResourceStream("LazyStack.Components.Config.Messages.json")!;
        // Add/Overwrite messages with messages in this library's Messages.json
        if (messagesStream != null)
        {
            using var messagesReader = new StreamReader(messagesStream);
            var messagesText = messagesReader.ReadToEnd();
            messages.MergeJson(messagesText);
        }
        return messages;
    }
}
