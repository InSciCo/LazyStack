﻿using LazyStack.Utils;
using LazyStack.ViewModels;
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
        var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
        var assemblyName = assembly!.GetName().Name;

        using var messagesStream = assembly.GetManifestResourceStream($"{assemblyName}.Config.Messages.json")!;
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