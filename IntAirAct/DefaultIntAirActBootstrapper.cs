using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using TinyIoC;

namespace IntAirAct
{
    public class DefaultIntAirActBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoC.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<Hello, Hello>().AsSingleton();
        }

        protected override TinyIoCContainer GetApplicationContainer()
        {
            return TinyIoCContainer.Current;
        }
    }
}
