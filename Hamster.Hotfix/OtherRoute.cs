using Hamster.Utils.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hamster.Hotfix
{
    public static class OtherRoute
    {
        public static IResponse Process(HttpListenerRequest request)
        {
            return new Text.Plane("404", 404);
        }
    }
}