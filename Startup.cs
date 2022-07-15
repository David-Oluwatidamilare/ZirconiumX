//using Microsoft.Owin;
//using Owin;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//[assembly: OwinStartup(typeof(ZirconiumX.Startup))]
//namespace ZirconiumX
//{
//    public partial class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            app.MapSignalR();
//        }
//    }
//}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ZirconiumX.Startup))]
namespace ZirconiumX
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
