using Hamster.Utils.Routing;
using System;
using System.Collections.Generic;
using System.Net;

namespace Hamster.Hotfix
{
    public static class Routes
    {
        public static Dictionary<string, IRoute> GetRoutes()
        {
            return new Dictionary<string, IRoute>()
            {
                {"/index.html",new IndexPage()}
            };
        }

        public static Func<HttpListenerRequest,IResponse> Other()
        {
            return OtherRoute.Process;
        }
    }
}
