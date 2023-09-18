using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyStack.ViewModels;

public static class LzViewModelFactory
{
    public static void RegisterAllLzFactories(IServiceCollection services, Assembly assembly)
    {
        Type[] iTypes = { typeof(ILzSingleton), typeof(ILzTransient), typeof(ILzScoped) };

        var factoryTypes = assembly
            .GetTypes()
            .Where(t => 
                iTypes.Any(iType =>  iType.IsAssignableFrom(t) && !t.IsAbstract));

        foreach (var type in factoryTypes)
        {
            var iTypeName = "I" + type.Name;
            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
                if (iface.Name.Equals(iTypeName))
                {
                    var registered = true;
                    if (typeof(ILzSingleton).IsAssignableFrom(type)) services.AddSingleton(iface, type);
                    else 
                    if (typeof(ILzTransient).IsAssignableFrom(type)) services.AddTransient(iface, type);
                    else
                    if (typeof(ILzScoped).IsAssignableFrom(type)) services.AddScoped(iface, type);
                    else registered = false;
                    if (registered)
                        Console.WriteLine($"Registered {type.Name}");

                }
        }
    }
}
