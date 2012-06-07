using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;

namespace IAIntAirAct
{
    public class Class1 : NancyModule
    {
        public Class1()
        {
            Get["/hello"] = parameters => "Hello World";
        }

    }
}
