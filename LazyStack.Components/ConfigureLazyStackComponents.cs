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

}
