using System.IO;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using LazyStack.Utils;

namespace LazyStackAuthV2
{
    public static class ConfigureLazyStackAuth
    {
        public static IServiceCollection AddLazyStackAuth(this IServiceCollection services)
        {
            // TryAdd only succeeds if the service is not already registered
            // It is used here to allow the calling programs to register their own
            // implementations of these classes.
            // Note: LzHost must be registered in the WASM Program.cs file so the current 
            // base url can be captured. MAUI programs are not loaded from a URL so they 
            // read their API params from a configuration file specific to the client build,
            // see the RunConfig class.
            services.TryAddSingleton<ILzHttpClient, LzHttpClient>();
            services.TryAddSingleton<IAuthProcess, AuthProcess>();
            services.TryAddSingleton<IAuthProviderCognito, AuthProviderCognito>();
            services.TryAddSingleton<ILoginFormat, LoginFormat>();
            services.TryAddSingleton<IEmailFormat, EmailFormat>();
            services.TryAddSingleton<IPhoneFormat, PhoneFormat>();
            services.TryAddSingleton<ICodeFormat, CodeFormat>();
            services.TryAddSingleton<IAuthProcess, AuthProcess>();
            services.TryAddSingleton<IPasswordFormat, PasswordFormat>();
            services.TryAddSingleton<ILzHost, LzHost>();
            return services;
        }

        public static IMessages AddlazyStackAuth(this IMessages messages)
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
}
