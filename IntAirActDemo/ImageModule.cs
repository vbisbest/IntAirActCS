using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;

namespace IntAirActDemo
{
    public class ImageModule : NancyModule
    {
        public ImageModule()
        {
            Get["/images"] = parameters => "Hello World";
        }
    }
}
