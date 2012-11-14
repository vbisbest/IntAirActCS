using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntAirAct
{
    public static class IARequestExtensions
    {
        public static string BodyAsString(this IARequest request)
        {
            return System.Text.Encoding.UTF8.GetString(request.Body);
        }
        
    }
}
