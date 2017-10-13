using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WaveAccountingIntegration.Startup))]
namespace WaveAccountingIntegration
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
