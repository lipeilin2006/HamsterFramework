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
    internal class IndexPage : IRoute
    {
        public IResponse Process(HttpListenerRequest request, MatchCollection matches)
        {
            return new Text("index");
        }
    }
}
