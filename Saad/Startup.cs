using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Saad.Startup))]
namespace Saad
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
