using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LazyStack.ViewModels;

public static class ConfigureLazyStackViewModels
{
    public static IServiceCollection AddLazyStackViewModels(this IServiceCollection services)
    {
        return services
            //.AddSingleton<DevConnectViewModel>()
            ;

    }
}
