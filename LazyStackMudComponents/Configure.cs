using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LazyStackAuth;

namespace LzMudComponents
{
    public static class Configure
    {
        public static IServiceCollection AddLzMudComponents(this IServiceCollection services, IConfiguration configuration)
        {
            return services
            .AddSingleton<ILoginFormat, LoginFormat>()
            .AddSingleton<IPasswordFormat, PasswordFormat>()
            .AddSingleton<IEmailFormat, EmailFormat>()
            .AddSingleton<IPhoneFormat, PhoneFormat>()
            .AddSingleton<ICodeFormat, CodeFormat>()
            // The following factory is a bit of a hack. We will clean this up later 
            // when we do the next version of LazyStackAuth. We will just have a 
            // IAwsConfig instead of IConfiguration, Aws,
            // UserPoolClientId, UserPoolId and IdentityPoolId.
            // This will eliminate the need for the factory.
            .AddSingleton<IAuthProvider, AuthProviderCognito>(
                (s) => new AuthProviderCognito(
                        s.GetService<IConfiguration>(),
                        s.GetService<ILoginFormat>(),
                        s.GetService<IPasswordFormat>(),
                        s.GetService<IEmailFormat>(),
                        s.GetService<ICodeFormat>(),
                        s.GetService<IPhoneFormat>(),
                        "AwsConfig", // the default is Aws
                        "UserPoolClientId", // the default is UserPoolClient
                        "UserPoolId",  // the default is UserPool
                        "IdentityPoolId" // the default is IdentityPool
                        ))
             .AddSingleton<IAuthProcess, AuthProcess>()
             //.AddMediatR(typeof(Configure).GetTypeInfo().Assembly)
             ;
        }
    }
}
