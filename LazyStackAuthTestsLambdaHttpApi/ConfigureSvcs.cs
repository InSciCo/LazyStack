using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace LambdaFunc
{
    /// <summary>
    /// This class follows the same semantics as that generated by LazyStack MDD
    /// </summary>
    public partial class Startup
    {
        public void ConfigureSvcs(IServiceCollection services)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddScoped<LazyStackAuthTestsController.ITestsController, LazyStackAuthTestsController.TestsControllerImpl>();
        }
    }
}
